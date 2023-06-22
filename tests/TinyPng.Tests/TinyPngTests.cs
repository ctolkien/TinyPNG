using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TinyPng.ResizeOperations;
using Xunit;

namespace TinyPng.Tests;

public class TinyPngTests
{
    private const string apiKey = "lolwat";

    internal const string Cat = "Resources/cat.jpg";
    internal const string CompressedCat = "Resources/compressedcat.jpg";
    internal const string ResizedCat = "Resources/resizedcat.jpg";
    internal const string SavedCatPath = "Resources/savedcat.jpg";

    [Fact]
    public void TinyPngClientThrowsWhenNoApiKeySupplied()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
    }

    [Fact]
    public void TinyPngClientThrowsWhenNoValidS3ConfigurationSupplied()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null));
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null, (AmazonS3Configuration)null));
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient("apiKey", (AmazonS3Configuration)null));
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(null, new AmazonS3Configuration("a", "b", "c", "d")));
    }

    [Fact]
    public void HandleScenarioOfExistingAuthHeaderOnTheClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Basic dGVzdDp0ZXN0");

        _ = new TinyPngClient("test", httpClient);

        //This just ensures that it doesn't throw
    }

    [Fact]
    public async Task Compression()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CanBeCalledMultipleTimesWithoutExploding()
    {
        TinyPngClient pngx1 = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result1 = await pngx1.Compress(Cat);

        Assert.Equal("image/jpeg", result1.Input.Type);
        Assert.Equal(400, result1.Output.Width);
        Assert.Equal(400, result1.Output.Height);


        TinyPngClient pngx2 = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result2 = await pngx2.Compress(Cat);

        Assert.Equal("image/jpeg", result2.Input.Type);
        Assert.Equal(400, result2.Output.Width);
        Assert.Equal(400, result2.Output.Height);
    }

    [Fact]
    public async Task CompressionCount()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        Assert.Equal(99, result.CompressionCount);
    }

    [Fact]
    public async Task CompressionWithBytes()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        byte[] bytes = await File.ReadAllBytesAsync(Cat);

        Responses.TinyPngCompressResponse result = await pngx.Compress(bytes);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CompressionWithStreams()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        await using FileStream fileStream = File.OpenRead(Cat);

        Responses.TinyPngCompressResponse result = await pngx.Compress(fileStream);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CompressionFromUrl()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(new Uri("https://sample.com/image.jpg"));

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public void CompressionShouldThrowIfNoPathToFile()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(string.Empty));
    }

    [Fact]
    public void CompressionShouldThrowIfNoUrlToFile()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(new Uri(string.Empty)));
    }

    [Fact]
    public void CompressionShouldThrowIfNoNonSuccessStatusCode()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().CompressAndFail()));

        _ = Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(Cat));
    }

    [Fact]
    public async Task CompressionAndDownload()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        byte[] downloadResult = await pngx.Compress(Cat)
            .Download()
            .GetImageByteData();

        Assert.Equal(16646, downloadResult.Length);
    }

    [Fact]
    public async Task CompressionAndDownloadAndGetUnderlyingStream()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        Stream downloadResult = await pngx.Compress(Cat)
            .Download()
            .GetImageStreamData();

        Assert.Equal(16646, downloadResult.Length);
    }

    [Fact]
    public async Task CompressionAndDownloadAndWriteToDisk()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));
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

    [Fact]
    public void ResizingOperationThrows()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(150, 150));
        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(null));
        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(Cat).Resize(null));

        Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Resize(150, 150));

        _ = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(Cat).Resize(0, 150));
        _ = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(Cat).Resize(150, 0));
    }

    [Fact]
    public void DownloadingOperationThrows()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Download());

        Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
        _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Download());
    }

    [Fact]
    public void DownloadingOperationThrowsOnNonSuccessStatusCode()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().DownloadAndFail()));

        _ = Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(Cat).Download());
    }

    [Fact]
    public async Task ResizingOperation()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(Cat).Resize(150, 150).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingScaleHeightOperation()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(Cat).Resize(new ScaleHeightResizeOperation(150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingScaleWidthOperation()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(Cat).Resize(new ScaleWidthResizeOperation(150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingFitResizeOperation()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(Cat).Resize(new FitResizeOperation(150, 150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingCoverResizeOperation()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(Cat).Resize(new CoverResizeOperation(150, 150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public void ResizingCoverResizeOperationThrowsWithInvalidParams()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        _ = Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(Cat).Resize(new CoverResizeOperation(0, 150)));
        _ = Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(Cat).Resize(new CoverResizeOperation(150, 0)));
    }

    [Fact]
    public void CompressAndStoreToS3ShouldThrowIfNoApiKeyProvided()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(string.Empty, new AmazonS3Configuration("a", "b", "c", "d")));
    }

    [Fact]
    public async Task CompressAndStoreToS3ShouldThrowIfS3HasNotBeenConfigured()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(null, "bucket/path.jpg"));
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, string.Empty));
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, "bucket/path.jpg"));
    }

    private const string ApiKey = "lolwat";
    private const string ApiAccessKey = "lolwat";

    [Fact]
    public async Task CompressAndStoreToS3()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        string sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result,
            new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
            "path.jpg")).ToString();

        Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);
    }

    [Fact]
    public async Task CompressAndStoreToS3FooBar()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().S3AndFail()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        _ = await Assert.ThrowsAsync<TinyPngApiException>(async () =>
            await pngx.SaveCompressedImageToAmazonS3(result,
            new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"), "path"));
    }

    [Fact]
    public async Task CompressAndStoreToS3Throws()
    {
        TinyPngClient pngx = new(apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, null, string.Empty));

        //S3 configuration has not been set
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, path: string.Empty));
    }

    [Fact]
    public async Task CompressAndStoreToS3WithOptionsPassedIntoConstructor()
    {
        TinyPngClient pngx = new(apiKey,
            new AmazonS3Configuration(ApiKey, ApiAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
            new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(Cat);
        string sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result, "path.jpg")).ToString();

        Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);
    }

    [Fact]
    public void TinyPngExceptionPopulatesCorrectData()
    {
        int StatusCode = 200;
        string StatusReasonPhrase = "status";
        string ErrorTitle = "title";
        string ErrorMessage = "message";
        TinyPngApiException e = new(StatusCode, StatusReasonPhrase, ErrorTitle, "message");

        string msg = $"Api Service returned a non-success status code when attempting an operation on an image: {StatusCode} - {StatusReasonPhrase}. {ErrorTitle}, {ErrorMessage}";

        Assert.Equal(StatusCode, e.StatusCode);
        Assert.Equal(StatusReasonPhrase, e.StatusReasonPhrase);
        Assert.Equal(ErrorTitle, e.ErrorTitle);
        Assert.Equal(msg, e.Message);
    }
}
