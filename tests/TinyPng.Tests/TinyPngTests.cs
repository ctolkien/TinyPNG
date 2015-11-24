using System.Threading.Tasks;
using Xunit;

namespace TinyPng.Tests
{
    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        [Fact(Skip ="interation")]
        public async Task Test()
        {
            var png = new TinyPngApi.TinyPng(apiKey);

            var result = await png.Shrink("Resources/cat.jpg");

            Assert.Equal(15423, result.Output.Size);

        }
    }
}
