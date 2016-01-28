using System.Threading.Tasks;
using Xunit;

namespace TinyPngApi.Tests
{
    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        [Fact(Skip ="integration")]
        public async Task Compression()
        {
            var pngx = new TinyPng(apiKey);


            var result = await pngx.Compress("Resources/cat.jpg");


            Assert.Equal("image/jpeg", result.Input.Type);

            Assert.Equal(300, result.Output.Width);

            Assert.Equal(182, (await result.GetImageByteData()).Length);
            

        }

        [Fact(Skip = "integration")]
        public async Task Resizing()
        {
            var pngx = new TinyPng(apiKey);

            var result = await pngx.Compress("Resources/cat.jpg");

            var resized = await pngx.Resize(result, new ScaleHeightResizeOperation(100));

            Assert.Equal(7085, (await resized.GetImageByteData()).Length);
            

        }
    }
}
