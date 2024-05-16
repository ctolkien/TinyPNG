using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
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
    /// <param name="backgroundTransform">Optional. Specify a hex value such as #000FFF when converting to a non-transparent image option</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TinyPngApiException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TinyPngConvertResponse> Convert(this Task<TinyPngCompressResponse> result, ConvertImageFormat convertOperation, string backgroundTransform = null)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }
        if (!string.IsNullOrEmpty(backgroundTransform) && (!backgroundTransform.StartsWith("#") || backgroundTransform.Length != 7))
        {
            throw new ArgumentOutOfRangeException(nameof(backgroundTransform), $"If {nameof(backgroundTransform)} is supplied, it should be a 6 character hex value, and include the hash");
        }

        TinyPngCompressResponse compressResponse = await result;

        string requestBody = JsonSerializer.Serialize(new { convert = new { type = convertOperation } }, TinyPngClient._jsonOptions);

        HttpRequestMessage msg = new(HttpMethod.Post, compressResponse.Output.Url)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await compressResponse._httpClient.SendAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            return new TinyPngConvertResponse(response);
        }

        ApiErrorResponse errorMsg = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(await response.Content.ReadAsStreamAsync(), TinyPngClient._jsonOptions);
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
