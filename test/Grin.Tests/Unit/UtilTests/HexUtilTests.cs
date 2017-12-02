using Common;
using Xunit;

namespace Grin.Tests.Unit.UtilTests
{
    public class HexUtilTests
    {
        [Fact]
        public void Test_from_hex()
        {
            Assert.Equal(HexUtil.from_hex("00000000"), new byte[] {0, 0, 0, 0});
            Assert.Equal(HexUtil.from_hex("0a0b0c0d"), new byte[] {10, 11, 12, 13});
            Assert.Equal(HexUtil.from_hex("000000ff"), new byte[] {0, 0, 0, 255});
        }

        [Fact]
        public void Test_to_hex()
        {
            Assert.Equal("00000000", HexUtil.to_hex(new byte[] {0, 0, 0, 0}));
            Assert.Equal("0a0b0c0d", HexUtil.to_hex(new byte[] {10, 11, 12, 13}));
            Assert.Equal("000000ff", HexUtil.to_hex(new byte[] {0, 0, 0, 255}));
        }
    }
}