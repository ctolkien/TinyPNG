using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TinyPng.Responses;

namespace TinyPng;

public static class ConvertExtensions
{
    /// <summary>
    /// Convert the image into another format
    /// </summary>
    /// <param name="result"></param>
    /// <param name="convertOperation"></param>
    /// <param name="backgroundTransform">Optional. Specify a hex value such as #000FFF when converting to a non-transparent image option
    /// You must specify a background color if you wish to convert an image with a transparent background to an image type which does not support transparency (like JPEG).</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TinyPngApiException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TinyPngConvertResponse> Convert(this Task<TinyPngCompressResponse> result, ConvertImageFormat convertOperation, string backgroundTransform = null, CancellationToken cancellationToken = default)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }
        if (!string.IsNullOrEmpty(backgroundTransform) && (!backgroundTransform.StartsWith("#", StringComparison.OrdinalIgnoreCase) || backgroundTransform.Length != 7))
        {
            throw new ArgumentOutOfRangeException(nameof(backgroundTransform), $"If {nameof(backgroundTransform)} is supplied, it should be a 6 character hex value, and include the hash");
        }

        TinyPngCompressResponse compressResponse = await result;

        string requestBody = JsonSerializer.Serialize(
            new
            {
                convert = new { type = convertOperation },
                transform = !string.IsNullOrEmpty(backgroundTransform) ? new { background = backgroundTransform } : null
            },
            TinyPngClient.JsonOptions);

        HttpRequestMessage msg = new(HttpMethod.Post, compressResponse.Output.Url)
        {
            Content = new JsonContent(requestBody)
        };

        HttpResponseMessage response = await compressResponse._httpClient.SendAsync(msg, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new TinyPngConvertResponse(response);
        }

        ApiErrorResponse errorMsg = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(await response.Content.ReadAsStreamAsync(), TinyPngClient.JsonOptions, cancellationToken);
        throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);


    }
}


public enum ConvertImageFormat
{
    /// <summary>
    /// By using wildcard, TinyPng will return the best format for the image.
    /// </summary>
    [EnumMember(Value = "*/*")]
    Wildcard,
    [EnumMember(Value = "image/webp")]
    WebP,
    [EnumMember(Value = "image/jpeg")]
    Jpeg,
    [EnumMember(Value = "image/png")]
    Png
}
