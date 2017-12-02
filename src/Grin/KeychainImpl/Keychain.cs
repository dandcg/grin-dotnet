using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Grin.KeychainImpl.Blind;
using Grin.KeychainImpl.ExtKey;
using Konscious.Security.Cryptography;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Serilog;

namespace Grin.KeychainImpl
{
    public class Keychain
    {
        public Secp256k1 Secp { get; }
        public ExtendedKey Extkey { get; }
        public Dictionary<string, SecretKey> KeyOverrides { get; }
        public ConcurrentDictionary<string, uint> KeyDerivationCache { get; }


        private Keychain(Secp256k1 secp, ExtendedKey extkey, Dictionary<string, SecretKey> keyOverrides,
            ConcurrentDictionary<string, uint> key_derivation_cache)
        {
            Secp = secp;
            Extkey = extkey;
            KeyOverrides = keyOverrides;
            KeyDerivationCache = key_derivation_cache;
        }


        public Identifier Root_key_id()
        {
            return Extkey.RootKeyId.Clone();
        }

        // For tests and burn only, associate a key identifier with a known secret key.
        //
        public static Keychain Burn_enabled(Keychain keychain, Identifier burnKeyId)
        {
            var keyOverridesNew =
                new Dictionary<string, SecretKey> {{burnKeyId.Hex, SecretKey.From_slice(keychain.Secp, new byte[32])}};
            return new Keychain(keychain.Secp, keychain.Extkey.Clone(), keyOverridesNew, keychain.KeyDerivationCache);
        }

        public static Keychain From_seed(byte[] seed)
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var extkey = ExtendedKey.from_seed(secp, seed);
            var keychain = new Keychain(
                    secp,
                    extkey,
                    new Dictionary<string, SecretKey>(), new ConcurrentDictionary<string, uint>())
                ;

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
            var extkeyNew = Extkey.Derive(Secp, derivation);
            var keyId = extkeyNew.Identifier(Secp);
            return keyId;
        }

        public SecretKey Derived_key(Identifier keyId)
        {
            Log.Verbose("Derived Key by key_id: {key_id}", keyId);

            // first check our overrides and just return the key if we have one in there
            KeyOverrides.TryGetValue(keyId.Hex, out var sk);

            if (sk != null)
            {
                Log.Verbose("... Derived Key (using override) key_id: {key_id}", keyId);
                return sk;
            }

            // then check the derivation cache to see if we have previously derived this key
            // if so use the derivation from the cache to derive the key

            var cache = KeyDerivationCache;

            if (cache.TryGetValue(keyId.Hex, out var derivation))
            {
         
                    Log.Verbose("... Derived Key (cache hit) key_id: {key_id}, derivation: {derivation}", keyId, derivation);
                    return derived_key_from_index(derivation);
              
            }


            // otherwise iterate over a large number of derivations looking for our key
            // cache the resulting derivations by key_id for faster lookup later
            // TODO - remove the 10k hard limit and be smarter about batching somehow


            for (uint i = 1; i <= 10000; i++)

            {
                var extkeyNew = Extkey.Derive(Secp, i);
                var ident = extkeyNew.Identifier(Secp);


                if (!cache.ContainsKey(ident.Hex))
                {
                    Log.Verbose("... Derived Key (cache miss) key_id: {key_id}, derivation: {derivation}", ident,
                        extkeyNew.NChild);
                    cache.TryAdd(ident.Hex, extkeyNew.NChild);
                }


                if (ident.Hex == keyId.Hex)
                {
                    return extkeyNew.Key;
                }
            }


            throw new Exception($"KeyDerivation - cannot find extkey for {keyId.Hex}");
        }

        // if we know the derivation index we can just straight to deriving the key
        public SecretKey derived_key_from_index(uint derivation)
        {
            Log.Verbose("Derived Key (fast) by derivation: {derivation}", derivation);
            var extkey = Extkey.Derive(Secp, derivation);
            return extkey.Key;
        }


        public Commitment Commit(ulong amount, Identifier keyId)
        {
            var skey = Derived_key(keyId);
            var commit = Secp.commit(amount, skey);

            return commit;
        }

        public Commitment commit_with_key_index(ulong amount, uint derivation)
        {

            var skey = derived_key_from_index(derivation);
            var commit = Secp.commit(amount, skey);
            return commit;

        }

        public Commitment Switch_commit(Identifier keyId)
        {
            var skey = Derived_key(keyId);
            var commit = Secp.switch_commit(skey);

            return commit;
        }

        public RangeProof Range_proof(ulong amount, Identifier keyId, Commitment commit, ProofMessage msg)

        {
            var skey = Derived_key(keyId);
            var rangeProof = Secp.range_proof(0, amount, skey, commit, msg);

            return rangeProof;
        }

        public ProofInfo Rewind_range_proof(Identifier keyId, Commitment commit, RangeProof proof)
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