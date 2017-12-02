using Common;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Transaction
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
    public class Output : IReadable, IWriteable, ICloneable<Output>
    {
        /// Options for an output's structure or use
        public OutputFeatures Features { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment Commit { get; set; }

        /// The switch commitment hash, a 160 bit length blake2 hash of blind*J
        public SwitchCommitHash SwitchCommitHash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof Proof { get; set; }


        /// Validates the range proof using the commitment
        public void Verify_proof(Secp256K1 secp)
        {
            secp.verify_range_proof(Commit, Proof);
        }

        /// Given the original blinding factor we can recover the
        /// value from the range proof and the commitment
        public ulong? Recover_value(Keychain keychain, Identifier keyId)
        {
            var pi = keychain.Rewind_range_proof(keyId, Commit, Proof);

            if (pi.Success)
            {
                return pi.Value;
            }

            return null;
        }

        public void Read(IReader reader)
        {
            Features = (OutputFeatures) reader.read_u8();
            Commit = Ser.Ser.ReadCommitment(reader);
            SwitchCommitHash = SwitchCommitHash.Readnew(reader);
            Proof = Ser.Ser.ReadRangeProof(reader);
        }

        public void Write(IWriter writer)
        {
            writer.write_u8((byte) Features);
            Commit.WriteCommitment(writer);
            SwitchCommitHash.Write(writer);

            if (writer.serialization_mode() == SerializationMode.Full)
            {
                writer.write_bytes(Proof.Proof);
            }
        }


        public Output Clone()
        {
            return new Output()
            {
                Features=Features,
                Commit=Commit.Clone(),
                SwitchCommitHash= SwitchCommitHash.Clone(),
                Proof=Proof.Clone()

            };
        }
    }
}