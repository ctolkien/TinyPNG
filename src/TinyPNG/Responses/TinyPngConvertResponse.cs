using System.Linq;
using System.Net.Http;

namespace TinyPng.Responses;

/// <summary>
/// Represents the response for converting an image using TinyPNG API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TinyPngConvertResponse"/> class.
/// </remarks>
/// <param name="msg">The HTTP response message.</param>
public class TinyPngConvertResponse(HttpResponseMessage msg) : TinyPngImageResponse(msg)
{
    /// <summary>
    /// Gets the content type of the converted image.
    /// </summary>
    public string ContentType => HttpResponseMessage.Content.Headers.ContentType.MediaType;

    /// <summary>
    /// Gets the height of the converted image.
    /// </summary>
    public string? ImageHeight => HttpResponseMessage.Content.Headers.GetValues("Image-Height").FirstOrDefault();

    /// <summary>
    /// Gets the width of the converted image.
    /// </summary>
    public string? ImageWidth => HttpResponseMessage.Content.Headers.GetValues("Image-Width").FirstOrDefault();
}
