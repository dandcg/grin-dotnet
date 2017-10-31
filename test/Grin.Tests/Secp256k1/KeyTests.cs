using System;
using System.IO;
using Grin.Secp256k1;
using Xunit;
//using Org.BouncyCastle.Asn1.Sec;
//using Org.BouncyCastle.Asn1.X9;
//using Org.BouncyCastle.Crypto.Parameters;
//using Org.BouncyCastle.Math;
//using Org.BouncyCastle.Math.EC;

namespace Grin.Tests.Secp256k1
{
    public class KeyTests
    {




        [Fact]
        public void skey_from_slice()
        {
    
           SecretKey sk=null;

           var ex = Assert.Throws<Exception>(() =>
           {
              sk = SecretKey.from_slice(new byte[31]);
           });

           Assert.Equal("InvalidSecretKey", ex.Message);
            
           sk = SecretKey.from_slice(new byte[32]);
            
           Assert.NotEmpty(sk.Value);



        }



        [Fact]
        public void pubkey_from_slice()
        {
            Exception ex = null;
            PublicKey pk = null;

            ex = Assert.Throws<Exception>(() =>
            {
                pk = PublicKey.from_slice(null);
            });
            Assert.Equal("InvalidPublicKey", ex.Message);

           ex = Assert.Throws<Exception>(() =>
            {
                pk = PublicKey.from_slice(new byte[]{1,2,3});
            });
            Assert.Equal("InvalidPublicKey", ex.Message);


          var uncompressed = PublicKey.from_slice(new byte[]{4, 54, 57, 149, 239, 162, 148, 175, 246, 254, 239, 75, 154, 152, 10, 82, 234, 224, 85, 220, 40, 100, 57, 121, 30, 162, 94, 156, 135, 67, 74, 49, 179, 57, 236, 53, 162, 124, 149, 144, 168, 77, 74, 30, 72, 211, 229, 110, 111, 55, 96, 193, 86, 227, 183, 152, 195, 155, 51, 247, 123, 113, 60, 228, 188});
           Assert.NotEmpty(uncompressed.Value);


           var  compressed = PublicKey.from_slice(new byte[] { 3, 23, 183, 225, 206, 31, 159, 148, 195, 42, 67, 115, 146, 41, 248, 140, 11, 3, 51, 41, 111, 180, 110, 143, 114, 134, 88, 73, 198, 174, 52, 184, 78});
            Assert.NotEmpty(compressed.Value);
        }



    }
}
