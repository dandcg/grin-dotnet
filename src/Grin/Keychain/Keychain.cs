using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;

namespace Grin.Keychain
{
//#[derive(Clone, Debug)]
    public class Keychain
    {
        public Secp256k1 secp { get; }
        public ExtendedKey extkey { get; }
        public Dictionary<byte[], SecretKey> key_overrides { get; }


        private Keychain(Secp256k1 secp, ExtendedKey extkey, Dictionary<byte[], SecretKey> key_overrides)
        {
            this.secp = secp;
            this.extkey = extkey;
            this.key_overrides = key_overrides;
        }


        public Identifier root_key_id()
        {
            return extkey.root_key_id.clone();
        }

        // For tests and burn only, associate a key identifier with a known secret key.
        //
        public static Keychain burn_enabled(Keychain keychain, Identifier burn_key_id)
        {
            var keyOverridesNew = new Dictionary<byte[], SecretKey>();
            keyOverridesNew.Add(
                burn_key_id.Bytes,
                SecretKey.from_slice(keychain.secp, new byte[32])
            );


            return new Keychain(keychain.secp, keychain.extkey.clone(), keyOverridesNew);
        }

        public static Keychain from_seed(byte[] seed)
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var extkey = ExtendedKey.from_seed(secp, seed);
            var keychain = new Keychain(
                secp,
                extkey,
                new Dictionary<byte[], SecretKey>());

            return keychain;
        }


        /// For testing - probably not a good idea to use outside of tests.
        public static Keychain from_random_seed()
        {
            var rng = RandomNumberGenerator.Create();

            var seedBytes = new byte[16];
            rng.GetBytes(seedBytes);

            var hashAlgorithm = new HMACBlake2B(null, 32 * 8);
            var seed = hashAlgorithm.ComputeHash(seedBytes);

            return from_seed(seed);
        }

        public Identifier derive_key_id(uint derivation)
        {
            var extkeyNew = extkey.derive(secp, derivation);
            var key_id = extkey.identifier(secp);

            return key_id;
        }

        public SecretKey derived_key(Identifier key_id)
        {
            Console.WriteLine(key_id.Hex);
            Console.WriteLine("---");

            key_overrides.TryGetValue(key_id.Bytes, out var sk);

            if (sk != null)
            {
                return sk;
            }

            for (uint i = 1; i <= 10000; i++)

            {
                var extkeyNew = extkey.derive(secp, i);
                var ident = extkeyNew.identifier(secp);

                Console.WriteLine(ident.Hex);

                if (ident.Hex== key_id.Hex)
                {
                    return extkeyNew.key;
                }
            }


            throw new Exception($"KeyDerivation - cannot find extkey for {key_id.Hex}");
        }

        public Commitment commit(ulong amount, Identifier key_id)
        {
            var skey = derived_key(key_id);
            var commit = secp.commit(amount, skey);

            return commit;
        }

        public Commitment switch_commit(Identifier key_id)
        {
            var skey = derived_key(key_id);
            var commit = secp.switch_commit(skey);

            return commit;
        }

        public RangeProof range_proof(
            ulong amount,
            Identifier key_id,
            Commitment commit,
            ProofMessage msg
        )

        {
            var skey = derived_key(key_id);
            var range_proof = secp.range_proof(0, amount, skey, commit, msg);

            return range_proof;
        }

        public ProofInfo rewind_range_proof(
            Identifier key_id,
            Commitment commit,
            RangeProof proof
        )
        {
            var nonce = derived_key(key_id);
            var proofInfo = secp.rewind_range_proof(commit, proof, nonce);
            return proofInfo;
        }

        public BlindingFactor blind_sum(BlindSum blind_sum)
        {
            throw new NotImplementedException();
            //var  pos_keys = blind_sum.positive_key_ids
            //                                       .iter()
            //                                       .filter_map(|k| self.derived_key(&k).ok())
            //    .collect();

            //var mut neg_keys: Vec<SecretKey> = blind_sum
            //                                       .negative_key_ids
            //                                       .iter()
            //                                       .filter_map(|k| self.derived_key(&k).ok())
            //    .collect();

            //pos_keys.extend(&blind_sum
            //                    .positive_blinding_factors
            //                    .iter()
            //                    .map(|b| b.secret_key())
            //    .collect::<Vec<SecretKey>>());

            //neg_keys.extend(&blind_sum
            //                    .negative_blinding_factors
            //                    .iter()
            //                    .map(|b| b.secret_key())
            //    .collect::<Vec<SecretKey>>());

            //var blinding = self.secp.blind_sum(pos_keys, neg_keys) ?;

            //Ok(BlindingFactor::new(blinding))
        }

        public Signiture sign(Message msg, Identifier key_id)
        {
            var skey = derived_key(key_id);
            var sig = secp.Sign(msg, skey);

            return sig;
        }

        public Signiture sign_with_blinding(Message msg, BlindingFactor blinding)

        {
            var sig = secp.Sign(msg, blinding.Key);

            return sig;
        }
    }
}