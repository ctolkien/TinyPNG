using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TinyPng.ResizeOperations;

namespace TinyPng.Tests
{
    [TestFixture]
    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        internal const string Cat = "Resources/cat.jpg";
        internal const string CompressedCat = "Resources/compressedcat.jpg";
        internal const string ResizedCat = "Resources/resizedcat.jpg";
        internal const string SavedCatPath = "Resources/savedcat.jpg";
        
        [Test]
        public void TinyPngClientThrowsWhenNoApiKeySupplied()
        {
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
        }
        
        [Test]
        public void TinyPngClientThrowsWhenNoValidS3ConfigurationSupplied()
        {
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null, null));
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient("apiKey", null));
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null, new AmazonS3Configuration("a", "b", "c", "d")));
        }
        
        [Test]
        public async Task Compression()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

            var result = await pngx.Compress(Cat);

            Assert.AreEqual("image/jpeg", result.Input.Type);
            Assert.AreEqual(400, result.Output.Width);
            Assert.AreEqual(400, result.Output.Height);
        }
        
        [Test]
        public async Task CanBeCalledMultipleTimesWihtoutExploding()
        {
            using (var pngx = new TinyPngClient(apiKey))
            {
                TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

                var result = await pngx.Compress(Cat);

                Assert.AreEqual("image/jpeg", result.Input.Type);
                Assert.AreEqual(400, result.Output.Width);
                Assert.AreEqual(400, result.Output.Height);
            }

            using (var pngx = new TinyPngClient(apiKey))
            {
                TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

                var result = await pngx.Compress(Cat);

                Assert.AreEqual("image/jpeg", result.Input.Type);
                Assert.AreEqual(400, result.Output.Width);
                Assert.AreEqual(400, result.Output.Height);
            }
        }
        
        [Test]
        public async Task CompressionCount()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

            var result = await pngx.Compress(Cat);

            Assert.AreEqual(99, result.CompressionCount);
        }
        
        [Test]
        public async Task CompressionWithBytes()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

            var bytes = File.ReadAllBytes(Cat);
            var result = await pngx.Compress(bytes);

            Assert.AreEqual("image/jpeg", result.Input.Type);
            Assert.AreEqual(400, result.Output.Width);
            Assert.AreEqual(400, result.Output.Height);
        }
        
        [Test]
        public async Task CompressionWithStreams()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

            using (var fileStream = File.OpenRead(Cat))
            {
                var result = await pngx.Compress(fileStream);

                Assert.AreEqual("image/jpeg", result.Input.Type);
                Assert.AreEqual(400, result.Output.Width);
                Assert.AreEqual(400, result.Output.Height);
            }
        }
        
        [Test]
        public void CompressionShouldThrowIfNoPathToFile()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().Compress());

            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(string.Empty));
        }
        
        [Test]
        public void CompressionShouldThrowIfNoNonSuccessStatusCode()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler().CompressAndFail());

            Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(Cat));
        }
        
        [Test]
        public async Task CompressionAndDownload()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Download());

            var downloadResult = await pngx.Compress(Cat)
                .Download()
                .GetImageByteData();

            Assert.AreEqual(16646, downloadResult.Length);
        }
        
        [Test]
        public async Task CompressionAndDownloadAndGetUnderlyingStream()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Download());

            var downloadResult = await pngx.Compress(Cat)
                .Download()
                .GetImageStreamData();

            Assert.AreEqual(16646, downloadResult.Length);
        }
        
        [Test]
        public async Task CompressionAndDownloadAndWriteToDisk()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Download());
            try
            {
                await pngx.Compress(Cat)
                .Download()
                .SaveImageToDisk(SavedCatPath);
            }
            finally
            {
                //try cleanup any saved file
                File.Delete(SavedCatPath);
            }

        }
        
        [Test]
        public void ResizingOperationThrows()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(150, 150));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(Cat).Resize(null));

            Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Resize(150, 150));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(Cat).Resize(0, 150));
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(Cat).Resize(150, 0));
        }
        
        [Test]
        public void DownloadingOperationThrows()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Download());

            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Download());

            Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Download());
        }
        
        [Test]
        public void DownloadingOperationThrowsOnNonSuccessStatusCode()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .DownloadAndFail());

            Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(Cat).Download());

        }
        
        [Test]
        public async Task ResizingOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var resizedImageByteData = await pngx.Compress(Cat).Resize(150, 150).GetImageByteData();

            Assert.AreEqual(5970, resizedImageByteData.Length);
        }
        
        [Test]
        public async Task ResizingScaleHeightOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var resizedImageByteData = await pngx.Compress(Cat).Resize(new ScaleHeightResizeOperation(150)).GetImageByteData();

            Assert.AreEqual(5970, resizedImageByteData.Length);
        }
        
        [Test]
        public async Task ResizingScaleWidthOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var resizedImageByteData = await pngx.Compress(Cat).Resize(new ScaleWidthResizeOperation(150)).GetImageByteData();

            Assert.AreEqual(5970, resizedImageByteData.Length);
        }
        
        [Test]
        public async Task ResizingFitResizeOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var result = await pngx.Compress(Cat);

            var resizedImageByteData = await pngx.Compress(Cat).Resize(new FitResizeOperation(150, 150)).GetImageByteData();

            Assert.AreEqual(5970, resizedImageByteData.Length);
        }
        
        [Test]
        public async Task ResizingCoverResizeOperation()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            var resizedImageByteData = await pngx.Compress(Cat).Resize(new CoverResizeOperation(150, 150)).GetImageByteData();

            Assert.AreEqual(5970, resizedImageByteData.Length);
        }
        
        [Test]
        public void ResizingCoverResizeOperationThrowsWithInvalidParams()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .Resize());

            Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(Cat).Resize(new CoverResizeOperation(0, 150)));
            Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(Cat).Resize(new CoverResizeOperation(150, 0)));
        }
        
        [Test]
        public void CompressAndStoreToS3ShouldThrowIfNoApiKeyProvided()
        {
            Assert.Throws<ArgumentNullException>(() => new TinyPngClient(string.Empty, new AmazonS3Configuration("a", "b", "c", "d")));
        }
        
        [Test]
        public async Task CompressAndStoreToS3ShouldThrowIfS3HasNotBeenConfigured()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(null, "bucket/path.jpg"));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, string.Empty));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, "bucket/path.jpg"));
        }

        private const string ApiKey = "lolwat";
        private const string ApiAccessKey = "lolwat";
        
        [Test]
        public async Task CompressAndStoreToS3()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            var sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result,
                new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
                "path.jpg")).ToString();

            Assert.AreEqual("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);
        }
        
        [Test]
        public async Task CompressAndStoreToS3FooBar()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3AndFail());

            var result = await pngx.Compress(Cat);

            Assert.ThrowsAsync<TinyPngApiException>(async () =>
                await pngx.SaveCompressedImageToAmazonS3(result,
                new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"), "path"));
        }
        
        [Test]
        public async Task CompressAndStoreToS3Throws()
        {
            var pngx = new TinyPngClient(apiKey);
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, null, string.Empty));

            //S3 configuration has not been set
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, path: string.Empty));
        }
        
        [Test]
        public async Task CompressAndStoreToS3WithOptionsPassedIntoConstructor()
        {
            var pngx = new TinyPngClient(apiKey, new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"));
            TinyPngClient.HttpClient = new HttpClient(new FakeResponseHandler()
                .Compress()
                .S3());

            var result = await pngx.Compress(Cat);
            var sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result, "path.jpg")).ToString();

            Assert.AreEqual("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);
        }
        
        [Test]
        public void TinyPngExceptionPopulatesCorrectData()
        {
            var StatusCode = 200;
            var StatusReasonPhrase = "status";
            var ErrorTitle = "title";
            var ErrorMessage = "message";
            var e = new TinyPngApiException(StatusCode, StatusReasonPhrase, ErrorTitle, "message");

            var msg = $"Api Service returned a non-success status code when attempting an operation on an image: {StatusCode} - {StatusReasonPhrase}. {ErrorTitle}, {ErrorMessage}";

            Assert.AreEqual(StatusCode, e.StatusCode);
            Assert.AreEqual(StatusReasonPhrase, e.StatusReasonPhrase);
            Assert.AreEqual(ErrorTitle, e.ErrorTitle);
            Assert.AreEqual(msg, e.Message);
        }
        
        [Test]
        public void CallingDisposeDoesNotBlowUpTheWorld()
        {
            var pngx = new TinyPngClient(apiKey);

            pngx.Dispose();
        }
    }
}
