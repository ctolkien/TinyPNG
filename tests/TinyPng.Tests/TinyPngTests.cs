using System;
using System.Threading.Tasks;
using Xunit;

namespace TinyPng.Tests
{
    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        const string Cat = "Resources/cat.jpg";

        [Fact(Skip = "Integration")]
        public async Task Compression()
        {
            var pngx = new TinyPngClient(apiKey);

            var result = await pngx.Compress(Cat);

            Assert.Equal("image/jpeg", result.Input.Type);

            Assert.Equal(300, result.Output.Width);

            Assert.Equal(182, (await result.GetImageByteData()).Length);
        }

        [Fact(Skip = "Integration")]
        public async Task Resizing()
        {
            var pngx = new TinyPngClient(apiKey);

            var result = await pngx.Compress(Cat);
            
            var resized = await pngx.Resize(result, new ScaleHeightResizeOperation(100));
            
            Assert.Equal(7111, (await resized.GetImageByteData()).Length);

        }

      

        [Fact(Skip ="Integration")]
        public async Task CompressAndStoreToS3ShouldThrowIfS3HasNotBeenConfigured()
        {
            var pngx = new TinyPngClient(apiKey);
            
            var result = await pngx.Compress(Cat);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, "bucket/path.jpg"));

        }

        private const string ApiKey = "lolwat";
        private const string ApiAccessKey = "lolwat";

        [Fact]
        public async Task CompressAndStoreToS3()
        {
            var pngx = new TinyPngClient(apiKey);

            var result = await pngx.Compress(Cat);

            var sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result, 
                new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"), 
                "path.jpg")).ToString();

            Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);

        }
    }
}
