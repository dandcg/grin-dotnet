using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;

namespace Grin.Keychain
{

    public class Keychain
    {
        public Secp256k1 Secp { get; }
        public ExtendedKey Extkey { get; }
        public Dictionary<string, SecretKey> KeyOverrides { get; }


        private Keychain(Secp256k1 secp, ExtendedKey extkey, Dictionary<string, SecretKey> keyOverrides)
        {
            Secp = secp;
            Extkey = extkey;
            KeyOverrides = keyOverrides;
        }


        public Identifier Root_key_id()
        {
            return Extkey.root_key_id.clone();
        }

        // For tests and burn only, associate a key identifier with a known secret key.
        //
        public static Keychain Burn_enabled(Keychain keychain, Identifier burnKeyId)
        {
            var keyOverridesNew = new Dictionary<string, SecretKey> {{burnKeyId.Hex, SecretKey.from_slice(keychain.Secp, new byte[32])}};


            return new Keychain(keychain.Secp, keychain.Extkey.clone(), keyOverridesNew);
        }

        public static Keychain From_seed(byte[] seed)
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var extkey = ExtendedKey.from_seed(secp, seed);
            var keychain = new Keychain(
                secp,
                extkey,
                new Dictionary<string, SecretKey>());

            return keychain;
        }


        /// For testing - probably not a good idea to use outside of tests.
        public static Keychain From_random_seed()
        {
            var rng = RandomNumberGenerator.Create();

            var seedBytes = new byte[16];
            rng.GetBytes(seedBytes);

            var hashAlgorithm = new HMACBlake2B(null, 32 * 8);
            var seed = hashAlgorithm.ComputeHash(seedBytes);

            return From_seed(seed);
        }

        public Identifier Derive_key_id(uint derivation)
        {
            var extkeyNew = Extkey.derive(Secp, derivation);
            var keyId = extkeyNew.identifier(Secp);
            return keyId;
        }

        public SecretKey Derived_key(Identifier keyId)
        {

            KeyOverrides.TryGetValue(keyId.Hex, out var sk);

            if (sk != null)
            {
                return sk;
            }

            for (uint i = 1; i <= 10000; i++)

            {
                var extkeyNew = Extkey.derive(Secp, i);
                var ident = extkeyNew.identifier(Secp);

                if (ident.Hex== keyId.Hex)
                {
                    return extkeyNew.key;
                }
            }


            throw new Exception($"KeyDerivation - cannot find extkey for {keyId.Hex}");
        }

        public Commitment Commit(ulong amount, Identifier key_id)
        {
            var skey = Derived_key(key_id);
            var commit = Secp.commit(amount, skey);

            return commit;
        }

        public Commitment Switch_commit(Identifier key_id)
        {
            var skey = Derived_key(key_id);
            var commit = Secp.switch_commit(skey);

            return commit;
        }

        public RangeProof Range_proof(ulong amount,Identifier keyId,Commitment commit,ProofMessage msg)

        {
            var skey = Derived_key(keyId);
            var rangeProof = Secp.range_proof(0, amount, skey, commit, msg);

            return rangeProof;
        }

        public ProofInfo Rewind_range_proof(Identifier keyId,Commitment commit,RangeProof proof)
        {
            var nonce = Derived_key(keyId);
            var proofInfo = Secp.rewind_range_proof(commit, proof, nonce);
            return proofInfo;
        }

        public BlindingFactor Blind_sum(BlindSum blindSum)
        {
            var posKeys = blindSum.positive_key_ids.Select(Derived_key).ToList();
            var negKeys = blindSum.negative_key_ids.Select(Derived_key).ToList();
            
            posKeys.AddRange(blindSum.positive_blinding_factors.Select(pbf => pbf.Key));
            negKeys.AddRange(blindSum.negative_blinding_factors.Select(nbf => nbf.Key));
            
            var blinding = Secp.blind_sum(posKeys.ToArray(), negKeys.ToArray());

            return BlindingFactor.New(blinding);
        }

        public Signiture Sign(Message msg, Identifier keyId)
        {
            var skey = Derived_key(keyId);
            var sig = Secp.Sign(msg, skey);
            return sig;
        }

        public Signiture Sign_with_blinding(Message msg, BlindingFactor blinding)
        {
            var sig = Secp.Sign(msg, blinding.Key);
            return sig;
        }
    }
}