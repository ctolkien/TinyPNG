using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TinyPng.ResizeOperations;
using TinyPng.Responses;

namespace TinyPng;

public static class ResizeExtensions
{
    /// <summary>
    /// Uses the TinyPng API to create a resized version of your uploaded image.
    /// </summary>
    /// <param name="result">This is the previous result of running a compression <see cref="TinyPngClient.Compress"/></param>
    /// <param name="resizeOperation">Supply a strongly typed Resize Operation. See <typeparamref name="CoverResizeOperation"/>, <typeparamref name="FitResizeOperation"/>, <typeparamref name="ScaleHeightResizeOperation"/>, <typeparamref name="ScaleWidthResizeOperation"/></param>
    /// <returns></returns>
    public static async Task<TinyPngResizeResponse> Resize(this Task<TinyPngCompressResponse> result, ResizeOperation resizeOperation)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (resizeOperation == null)
        {
            throw new ArgumentNullException(nameof(resizeOperation));
        }

        TinyPngCompressResponse compressResponse = await result;

        string requestBody = JsonSerializer.Serialize(new { resize = resizeOperation }, TinyPngClient.JsonOptions);

        HttpRequestMessage msg = new(HttpMethod.Post, compressResponse.Output.Url)
        {
            Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await compressResponse.HttpClient.SendAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            return new TinyPngResizeResponse(response);
        }

        ApiErrorResponse errorMsg = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(await response.Content.ReadAsStreamAsync(), TinyPngClient.JsonOptions);
        throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
    }

    /// <summary>
    /// Uses the TinyPng API to create a resized version of your uploaded image.
    /// </summary>
    /// <param name="result">This is the previous result of running a compression <see cref="TinyPngClient.Compress"/></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="resizeType"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static async Task<TinyPngResizeResponse> Resize(this Task<TinyPngCompressResponse> result, int width, int height, ResizeType resizeType = ResizeType.Fit)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (width == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be 0");
        }

        if (height == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be 0");
        }

        ResizeOperation resizeOp = new(resizeType, width, height);

        return await result.Resize(resizeOp);
    }
}
