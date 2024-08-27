using System;
using System.Net.Http;

namespace TinyPng.Responses;

/// <summary>
/// Represents the response from the TinyPNG compression API.
/// </summary>
public class TinyPngCompressResponse : TinyPngResponse
{
    /// <summary>
    /// Gets the input details of the compressed image.
    /// </summary>
    public TinyPngApiInput Input { get; private set; }

    /// <summary>
    /// Gets the output details of the compressed image.
    /// </summary>
    public TinyPngApiOutput Output { get; private set; }
    internal HttpClient HttpClient { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TinyPngCompressResponse"/> class.
    /// </summary>
    /// <param name="msg">The HTTP response message.</param>
    /// <param name="apiResult">The result of the API call.</param>
    public TinyPngCompressResponse(HttpResponseMessage msg, TinyPngApiResult apiResult, HttpClient httpClient) : base(msg)
    {
        if (msg is null)
        {
            throw new ArgumentNullException(nameof(msg));
        }

        if (apiResult is null)
        {
            throw new ArgumentNullException(nameof(apiResult));
        }

        Input = apiResult.Input;
        Output = apiResult.Output;
        HttpClient = httpClient;
    }
}
