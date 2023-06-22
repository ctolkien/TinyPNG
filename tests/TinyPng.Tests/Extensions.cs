using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace TinyPng.Tests;

internal static class Extensions
{
    public static FakeResponseHandler Compress(this FakeResponseHandler fakeResponse)
    {
        TinyPngApiResult content = new()
        {
            Input = new TinyPngApiInput
            {
                Size = 18031,
                Type = "image/jpeg"
            },
            Output = new TinyPngApiOutput
            {
                Width = 400,
                Height = 400,
                Size = 16646,
                Type = "image/jpeg",
                Ratio = 0.9232f,
                Url = "https://api.tinify.com/output"
            }
        };
        HttpResponseMessage compressResponseMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Content = new StringContent(JsonSerializer.Serialize(content)),
        };
        compressResponseMessage.Headers.Location = new Uri("https://api.tinify.com/output");
        compressResponseMessage.Headers.Add("Compression-Count", "99");

        fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/shrink"), compressResponseMessage);
        return fakeResponse;
    }

    public static FakeResponseHandler CompressAndFail(this FakeResponseHandler fakeResponse)
    {
        TinyPngApiException errorApiObject = new(400, "reason", "title", "message");

        HttpResponseMessage compressResponseMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.BadRequest,
            Content = new StringContent(JsonSerializer.Serialize(errorApiObject))
        };
        fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/shrink"), compressResponseMessage);
        return fakeResponse;
    }

    public static FakeResponseHandler Download(this FakeResponseHandler fakeResponse)
    {
        FileStream compressedCatStream = File.OpenRead(TinyPngTests.CompressedCat);
        HttpResponseMessage outputResponseMessage = new()
        {
            Content = new StreamContent(compressedCatStream),
            StatusCode = System.Net.HttpStatusCode.OK
        };

        fakeResponse.AddFakeGetResponse(new Uri("https://api.tinify.com/output"), outputResponseMessage);
        return fakeResponse;
    }

    public static FakeResponseHandler DownloadAndFail(this FakeResponseHandler fakeResponse)
    {
        HttpResponseMessage outputResponseMessage = new()
        {
            Content = new StringContent(JsonSerializer.Serialize(new Responses.ApiErrorResponse { Error = "Stuff's on fire yo!", Message = "This is the error message" })),
            StatusCode = System.Net.HttpStatusCode.InternalServerError
        };

        fakeResponse.AddFakeGetResponse(new Uri("https://api.tinify.com/output"), outputResponseMessage);
        return fakeResponse;
    }

    public static FakeResponseHandler Resize(this FakeResponseHandler fakeResponse)
    {
        FileStream resizedCatStream = File.OpenRead(TinyPngTests.ResizedCat);
        HttpResponseMessage resizeMessage = new()
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
        HttpResponseMessage amazonMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.OK
        };
        amazonMessage.Headers.Add("Location", "https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg");

        fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/output"), amazonMessage);
        return fakeResponse;
    }

    public static FakeResponseHandler S3AndFail(this FakeResponseHandler fakeResponse)
    {
        HttpResponseMessage amazonMessage = new()
        {
            Content = new StringContent(JsonSerializer.Serialize(new Responses.ApiErrorResponse { Error = "Stuff's on fire yo!", Message = "This is the error message" })),
            StatusCode = System.Net.HttpStatusCode.BadRequest
        };
        //amazonMessage.Headers.Add("Location", "https://s3-ap-southeast-2.amazonaws.com/tinypng-test-bucket/path.jpg");

        fakeResponse.AddFakePostResponse(new Uri("https://api.tinify.com/output"), amazonMessage);
        return fakeResponse;
    }
}
