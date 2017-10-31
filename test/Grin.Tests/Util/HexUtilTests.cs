using Grin.Util;
using Xunit;

namespace Grin.Tests.Util
{
    public class HexUtilTests
    {
        [Fact]
        public void test_from_hex()
        {
            Assert.Equal(HexUtil.from_hex("00000000"), new byte[] {0, 0, 0, 0});
            Assert.Equal(HexUtil.from_hex("0a0b0c0d"), new byte[] {10, 11, 12, 13});
            Assert.Equal(HexUtil.from_hex("000000ff"), new byte[] {0, 0, 0, 255});
        }

        [Fact]
        public void test_to_hex()
        {
            Assert.Equal(HexUtil.to_hex(new byte[] {0, 0, 0, 0}), "00000000");
            Assert.Equal(HexUtil.to_hex(new byte[] {10, 11, 12, 13}), "0a0b0c0d");
            Assert.Equal(HexUtil.to_hex(new byte[] {0, 0, 0, 255}), "000000ff");
        }
    }
}