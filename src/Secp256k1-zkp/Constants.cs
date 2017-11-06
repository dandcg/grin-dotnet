﻿namespace Secp256k1Proxy
{
    public static class Constants
    {


        //! # Constants
        //! Constants related to the API and the underlying curve

        /// The size (in bytes) of a message
        public const int MESSAGE_SIZE = 32;

        /// The size (in bytes) of a secret key
        public const int SECRET_KEY_SIZE = 32;

        /// The size (in bytes) of a public key array. This only needs to be 65
        /// but must be 72 for compatibility with the `ArrayVec` library.
        public const int PUBLIC_KEY_SIZE = 72;

        /// The size (in bytes) of an uncompressed public key
        public const int UNCOMPRESSED_PUBLIC_KEY_SIZE = 65;

        /// The size (in bytes) of a compressed public key
        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;

        /// The maximum size of a signature
        public const int MAX_SIGNATURE_SIZE = 72;

        /// The maximum size of a compact signature
        public const int COMPACT_SIGNATURE_SIZE = 64;

        /// The size of a Pedersen commitment
        public const int PEDERSEN_COMMITMENT_SIZE = 33;

        /// The max size of a range proof
        public const int MAX_PROOF_SIZE = 5134;

        /// The maximum size of a message embedded in a range proof
// pub const PROOF_MSG_SIZE: usize = 4096;
        public const int PROOF_MSG_SIZE = 2048;

        public static readonly byte[] CURVE_ORDER = new byte[32]
        {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe,
            0xba, 0xae, 0xdc, 0xe6, 0xaf, 0x48, 0xa0, 0x3b,
            0xbf, 0xd2, 0x5e, 0x8c, 0xd0, 0x36, 0x41, 0x41
        };

        /// The X coordinate of the generator
        public static readonly byte[] GENERATOR_X = new byte[32]
        {
            0x79, 0xbe, 0x66, 0x7e, 0xf9, 0xdc, 0xbb, 0xac,
            0x55, 0xa0, 0x62, 0x95, 0xce, 0x87, 0x0b, 0x07,
            0x02, 0x9b, 0xfc, 0xdb, 0x2d, 0xce, 0x28, 0xd9,
            0x59, 0xf2, 0x81, 0x5b, 0x16, 0xf8, 0x17, 0x98
        };

        /// The Y coordinate of the generator
        public static readonly byte[] GENERATOR_Y = new byte[32]
        {
            0x48, 0x3a, 0xda, 0x77, 0x26, 0xa3, 0xc4, 0x65,
            0x5d, 0xa4, 0xfb, 0xfc, 0x0e, 0x11, 0x08, 0xa8,
            0xfd, 0x17, 0xb4, 0x48, 0xa6, 0x85, 0x54, 0x19,
            0x9c, 0x47, 0xd0, 0x8f, 0xfb, 0x10, 0xd4, 0xb8
        };

        public static readonly byte[] GENERATOR_H = new byte[33] {
            0x11,
            0x50, 0x92, 0x9b, 0x74, 0xc1, 0xa0, 0x49, 0x54,
            0xb7, 0x8b, 0x4b, 0x60, 0x35, 0xe9, 0x7a, 0x5e,
            0x07, 0x8a, 0x5a, 0x0f, 0x28, 0xec, 0x96, 0xd5,
            0x47, 0xbf, 0xee, 0x9a, 0xce, 0x80, 0x3a, 0xc0
        };

        public static readonly byte[] GENERATOR_J = new byte[33] {
            0x11,
            0xb8, 0x60, 0xf5, 0x67, 0x95, 0xfc, 0x03, 0xf3,
            0xc2, 0x16, 0x85, 0x38, 0x3d, 0x1b, 0x5a, 0x2f,
            0x29, 0x54, 0xf4, 0x9b, 0x7e, 0x39, 0x8b, 0x8d,
            0x2a, 0x01, 0x93, 0x93, 0x36, 0x21, 0x15, 0x5f
        };
    }
}
