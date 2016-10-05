using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace TinyPng.Tests
{
    static class Extensions
    {
        public static FakeResponseHandler Compress(this FakeResponseHandler fakeResponse)
        {

            var content = new TinyPngApiResult();
            content.Input = new TinyPngApiInput
            {
                Size = 18031,
                Type = "image/jpeg"
            };
            content.Output = new TinyPngApiOutput
            {
                Width = 400,
                Height = 400,
                Size = 16646,
                Type = "image/jpeg",
                Ratio = 0.9232f,
                Url = "https://api.tinify.com/output"
            };

            var compressResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Content = new StringContent(JsonConvert.SerializeObject(content)),
            };
            compressResponseMessage.Headers.Location = new Uri("https://api.tinify.com/output");
            compressResponseMessage.Headers.Add("Compression-Count", "99");

            fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/shrink"), compressResponseMessage);
            return fakeResponse;
        }
        public static FakeResponseHandler Download(this FakeResponseHandler fakeResponse)
        {
            var compressedCatStream = File.OpenRead(TinyPngTests.CompressedCat);
            var outputResponseMessage = new HttpResponseMessage
            {
                Content = new StreamContent(compressedCatStream),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            fakeResponse.AddFakeGetResponse(new Uri("https://api.tinify.com/output"), outputResponseMessage);
            return fakeResponse;
        }

        public static FakeResponseHandler Resize(this FakeResponseHandler fakeResponse)
        {
            var resizedCatStream = File.OpenRead(TinyPngTests.ResizedCat);
            var resizeMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(resizedCatStream)
            };
            resizeMessage.Headers.Add("Image-Width", "150");
            resizeMessage.Headers.Add("Image-Height", "150");

            fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/output"), resizeMessage);
            return fakeResponse;
        }

        public static FakeResponseHandler S3(this FakeResponseHandler fakeResponse)
        {
            var amazonMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
            amazonMessage.Headers.Add("Location", "https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg");

            fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/output"), amazonMessage);
            return fakeResponse;
        }
    }

    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        internal const string Cat = "Resources/cat.jpg";
        internal const string CompressedCat = "Resources/compressedcat.jpg";
        internal const string ResizedCat = "Resources/resizedcat.jpg";


        [Fact]
        public void TinyPngClientThrowsWhenNoApiKeySupplied()
        {
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
        }

        [Fact]
        public void TinyPngClientThrowsWhenNoValidS3ConfigurationSupplied()
        {
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient("apiKey", null));
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null, new AmazonS3Configuration("a", "b", "c", "d")));
        }


        [Fact]
        public async Task Compression()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler().Compress());

            var result = await pngx.Compress(Cat);

            Assert.Equal("image/jpeg", result.Input.Type);
            Assert.Equal(400, result.Output.Width);
            Assert.Equal(400, result.Output.Height);
        }

        [Fact]
        public async Task CompressionWithBytes()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler().Compress());

            var bytes = File.ReadAllBytes(Cat);
            var result = await pngx.Compress(bytes);

            Assert.Equal("image/jpeg", result.Input.Type);
            Assert.Equal(400, result.Output.Width);
            Assert.Equal(400, result.Output.Height);
        }

        [Fact]
        public async Task CompressionWithStreams()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler().Compress());

            var fileStream = File.OpenRead(Cat);

            var result = await pngx.Compress(fileStream);

            Assert.Equal("image/jpeg", result.Input.Type);
            Assert.Equal(400, result.Output.Width);
            Assert.Equal(400, result.Output.Height);
        }

        [Fact]
        public async Task CompressionShouldThrowIfNoPathToFile()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler().Compress());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(string.Empty));
        }

        [Fact]
        public async Task CompressionAndDownload()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Download());

            var result = await pngx.Compress(Cat);

            var downloadResult = await pngx.Download(result);

            Assert.Equal(16646, (await downloadResult.GetImageByteData()).Length);
        }

        [Fact]
        public async Task ResizingOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resized = await pngx.Resize(result, 150, 150);

            var resizedImageByteData = await resized.GetImageByteData();

            Assert.Equal(5970, resizedImageByteData.Length);

        }

        [Fact]
        public async Task ResizingOperationThrows()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Resize(null, 150, 150));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Resize(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Resize(result, null));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Resize(result, 0, 150));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Resize(result, 150, 0));
            
        }

        [Fact]
        public async Task ResizingScaleHeightOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resized = await pngx.Resize(result, new ScaleHeightResizeOperation(150));

            var resizedImageByteData = await resized.GetImageByteData();

            Assert.Equal(5970, resizedImageByteData.Length);

        }

        [Fact]
        public async Task ResizingScaleWidthOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resized = await pngx.Resize(result, new ScaleWidthResizeOperation(150));

            var resizedImageByteData = await resized.GetImageByteData();

            Assert.Equal(5970, resizedImageByteData.Length);

        }

        [Fact]
        public async Task ResizingFitResizeOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resized = await pngx.Resize(result, new FitResizeOperation(150, 150));

            var resizedImageByteData = await resized.GetImageByteData();

            Assert.Equal(5970, resizedImageByteData.Length);

        }

        [Fact]
        public async Task ResizingCoverResizeOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resized = await pngx.Resize(result, new CoverResizeOperation(150, 150));

            var resizedImageByteData = await resized.GetImageByteData();

            Assert.Equal(5970, resizedImageByteData.Length);

        }

        [Fact]
        public async Task ResizingCoverResizeOperationThrowsWithInvalidParams()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            await Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Resize(result, new CoverResizeOperation(0, 150)));
            await Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Resize(result, new CoverResizeOperation(150, 0)));

        }


        [Fact]
        public async Task CompressAndStoreToS3ShouldThrowIfS3HasNotBeenConfigured()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(null, "bucket/path.jpg"));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, string.Empty));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, "bucket/path.jpg"));

        }

        private const string ApiKey = "lolwat";
        private const string ApiAccessKey = "lolwat";

        [Fact]
        public async Task CompressAndStoreToS3()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            var sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result,
                new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
                "path.jpg")).ToString();

            Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);

        }

        [Fact]
        public async Task CompressAndStoreToS3Throws()
        {
            var pngx = new TinyPngClient(apiKey);
            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, path: string.Empty));

        }

        [Fact]
        public async Task CompressAndStoreToS3WithOptionsPassedIntoConstructor()
        {
            var pngx = new TinyPngClient(apiKey, new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"));

            pngx.httpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            var sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result, "path.jpg")).ToString();

            Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);

        }

        [Fact]
        public void TinyPngExceptionPopulatesCorrectData()
        {
            var e = new TinyPngApiException(200, "status", "title", "message");

            Assert.Equal(200, e.StatusCode);
            Assert.Equal("status", e.StatusReasonPhrase);
            Assert.Equal("title", e.ErrorTitle);
            Assert.Equal("message", e.Message);
        }
    }
}
