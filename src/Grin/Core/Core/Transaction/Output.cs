using Grin.Core.Ser;
using Grin.Keychain.ExtKey;
using Secp256k1Proxy;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.Core.Core.Transaction
{
    /// Output for a transaction, defining the new ownership of coins that are being
    /// transferred. The commitment is a blinded value for the output while the
    /// range proof guarantees the commitment includes a positive value without
    /// overflow and the ownership of the private key. The switch commitment hash
    /// provides future-proofing against quantum-based attacks, as well as provides
    /// wallet implementations with a way to identify their outputs for wallet
    /// reconstruction
    /// 
    /// The hash of an output only covers its features, lock_height, commitment,
    /// and switch commitment. The range proof is expected to have its own hash
    /// and is stored and committed to separately.
    public class Output : IReadable, IWriteable
    {
    
        public Output()
        {
            
        }

        /// Options for an output's structure or use
        public OutputFeatures features { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment commit { get; set; }

        /// The switch commitment hash, a 160 bit length blake2 hash of blind*J
        public SwitchCommitHash switch_commit_hash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof proof { get; set; }


        /// Validates the range proof using the commitment
        public void Verify_proof(Secp256k1 secp)
        {
            secp.verify_range_proof(commit, proof);
        }

        /// Given the original blinding factor we can recover the
        /// value from the range proof and the commitment
        public ulong? Recover_value(Keychain.Keychain.Keychain keychain, Identifier keyId)
        {
            var pi = keychain.Rewind_range_proof(keyId, commit, proof);

            if (pi.success)
            {
                return pi.value;
            }

            return null;
        }

        public void read(IReader reader)
        {
            features = (OutputFeatures) reader.read_u8();
            commit = Ser.Ser.ReadCommitment(reader);
            switch_commit_hash = SwitchCommitHash.readnew(reader);
            proof = Ser.Ser.ReadRangeProof(reader);
        }

        public void write(IWriter writer)
        {
            writer.write_u8((byte) features);
            commit.WriteCommitment(writer);
            switch_commit_hash.write(writer);

            if (writer.serialization_mode() == SerializationMode.Full)
            {
                writer.write_bytes(proof.Proof);
            }
        }


        public Output Clone()
        {
            return new Output()
            {
                features=features,
                commit=commit.Clone(),
                switch_commit_hash= switch_commit_hash.Clone(),
                proof=proof.Clone()

            };
        }
    }
}