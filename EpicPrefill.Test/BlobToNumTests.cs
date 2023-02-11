using EpicPrefill.Extensions;

namespace EpicPrefill.Test
{
    public sealed class BlobToNumTests
    {
        [Theory]
        [InlineData("043", 43)]
        [InlineData("013000000000", 13)]
        [InlineData("056217002000000000000000", 186680)]
        [InlineData("002120006000000000000000", 423938)]
        [InlineData("181145061044190114148071", 5157873634357645749)]
        [InlineData("070046007000000000000000", 470598)]
        [InlineData("193157029079132122245178", 12895347816726896065)]
        [InlineData("133146006000000000000000", 430725)]
        public void InputBlobParsesCorrectly(string input, ulong expected)
        {
            var result = input.BlobToNum();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BlobToNumTest()
        {
            var inputString = "056217002000000000000000";
            var result = inputString.BlobToNum();

            Assert.Equal(186680u, result);
        }
    }
}