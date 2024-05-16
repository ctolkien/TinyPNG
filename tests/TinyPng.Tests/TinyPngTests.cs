using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TinyPng.ResizeOperations;
using Xunit;

namespace TinyPng.Tests;

public class TinyPngTests
{
    private const string _apiKey = "lolwat";

    internal const string _cat = "Resources/cat.jpg";
    internal const string _compressedCat = "Resources/compressedcat.jpg";
    internal const string _resizedCat = "Resources/resizedcat.jpg";
    internal const string _savedCatPath = "Resources/savedcat.jpg";

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
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CanBeCalledMultipleTimesWithoutExploding()
    {
        TinyPngClient pngx1 = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result1 = await pngx1.Compress(_cat);

        Assert.Equal("image/jpeg", result1.Input.Type);
        Assert.Equal(400, result1.Output.Width);
        Assert.Equal(400, result1.Output.Height);


        TinyPngClient pngx2 = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result2 = await pngx2.Compress(_cat);

        Assert.Equal("image/jpeg", result2.Input.Type);
        Assert.Equal(400, result2.Output.Width);
        Assert.Equal(400, result2.Output.Height);
    }

    [Fact]
    public async Task CompressionCount()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        Assert.Equal(99, result.CompressionCount);
    }

    [Fact]
    public async Task CompressionWithBytes()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        byte[] bytes = await File.ReadAllBytesAsync(_cat);

        Responses.TinyPngCompressResponse result = await pngx.Compress(bytes);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CompressionWithStreams()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        await using FileStream fileStream = File.OpenRead(_cat);

        Responses.TinyPngCompressResponse result = await pngx.Compress(fileStream);

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CompressionFromUrl()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(new Uri("https://sample.com/image.jpg"));

        Assert.Equal("image/jpeg", result.Input.Type);
        Assert.Equal(400, result.Output.Width);
        Assert.Equal(400, result.Output.Height);
    }

    [Fact]
    public async Task CompressionShouldThrowIfNoPathToFile()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress()));

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(string.Empty));
    }

    [Fact]
    public async Task CompressionShouldThrowIfNoNonSuccessStatusCode()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().CompressAndFail()));

        await Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(_cat));
    }

    [Fact]
    public async Task CompressionAndDownload()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        byte[] downloadResult = await pngx.Compress(_cat)
            .Download()
            .GetImageByteData();

        Assert.Equal(16646, downloadResult.Length);
    }

    [Fact]
    public async Task CompressionAndDownloadAndGetUnderlyingStream()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        Stream downloadResult = await pngx.Compress(_cat)
            .Download()
            .GetImageStreamData();

        Assert.Equal(16646, downloadResult.Length);
    }

    [Fact]
    public async Task CompressionAndDownloadAndWriteToDisk()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));
        try
        {
            await pngx.Compress(_cat)
            .Download()
            .SaveImageToDisk(_savedCatPath);
        }
        finally
        {
            //try cleanup any saved file
            File.Delete(_savedCatPath);
        }
    }

    [Fact]
    public async Task ResizingOperationThrows()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(150, 150));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Resize(null));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress(_cat).Resize(null));

        Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Resize(150, 150));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(_cat).Resize(0, 150));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pngx.Compress(_cat).Resize(150, 0));
    }

    [Fact]
    public async Task DownloadingOperationThrows()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Download()));

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.Compress((string)null).Download());

        Task<Responses.TinyPngCompressResponse> nullCompressResponse = null;
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await nullCompressResponse.Download());
    }

    [Fact]
    public async Task DownloadingOperationThrowsOnNonSuccessStatusCode()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().DownloadAndFail()));

        await Assert.ThrowsAsync<TinyPngApiException>(async () => await pngx.Compress(_cat).Download());
    }

    [Fact]
    public async Task ResizingOperation()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(_cat).Resize(150, 150).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingScaleHeightOperation()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(_cat).Resize(new ScaleHeightResizeOperation(150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingScaleWidthOperation()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(_cat).Resize(new ScaleWidthResizeOperation(150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingFitResizeOperation()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(_cat).Resize(new FitResizeOperation(150, 150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingCoverResizeOperation()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        byte[] resizedImageByteData = await pngx.Compress(_cat).Resize(new CoverResizeOperation(150, 150)).GetImageByteData();

        Assert.Equal(5970, resizedImageByteData.Length);
    }

    [Fact]
    public async Task ResizingCoverResizeOperationThrowsWithInvalidParams()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().Resize()));

        await Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(_cat).Resize(new CoverResizeOperation(0, 150)));
        await Assert.ThrowsAsync<ArgumentException>(async () => await pngx.Compress(_cat).Resize(new CoverResizeOperation(150, 0)));
    }

    [Fact]
    public void CompressAndStoreToS3ShouldThrowIfNoApiKeyProvided()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TinyPngClient(string.Empty, new AmazonS3Configuration("a", "b", "c", "d")));
    }

    [Fact]
    public async Task CompressAndStoreToS3ShouldThrowIfS3HasNotBeenConfigured()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(null, "bucket/path.jpg"));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, string.Empty));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, "bucket/path.jpg"));
    }

    private const string _awsAccessKeyId = "lolwat";
    private const string _awsSecretAccessKey = "lolwat";

    [Fact]
    public async Task CompressAndStoreToS3()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        string sendToAmazon = (await pngx.SaveCompressedImageToAmazonS3(result,
            new AmazonS3Configuration(_awsAccessKeyId, _awsSecretAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
            "path.jpg")).ToString();

        Assert.Equal("https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg", sendToAmazon);
    }

    [Fact]
    public async Task CompressAndStoreToS3FooBar()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().S3AndFail()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        await Assert.ThrowsAsync<TinyPngApiException>(async () =>
            await pngx.SaveCompressedImageToAmazonS3(result,
            new AmazonS3Configuration(_awsAccessKeyId, _awsSecretAccessKey, "tinypng-test-bucket", "ap-southeast-2"), "path"));
    }

    [Fact]
    public async Task CompressAndStoreToS3Throws()
    {
        TinyPngClient pngx = new(_apiKey, new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, null, string.Empty));

        //S3 configuration has not been set
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pngx.SaveCompressedImageToAmazonS3(result, path: string.Empty));
    }

    [Fact]
    public async Task CompressAndStoreToS3WithOptionsPassedIntoConstructor()
    {
        TinyPngClient pngx = new(_apiKey,
            new AmazonS3Configuration(_awsAccessKeyId, _awsSecretAccessKey, "tinypng-test-bucket", "ap-southeast-2"),
            new HttpClient(new FakeResponseHandler().Compress().S3()));

        Responses.TinyPngCompressResponse result = await pngx.Compress(_cat);
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
