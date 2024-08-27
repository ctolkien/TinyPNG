using System.Net.Http;

namespace TinyPng.Responses;


/// <summary>
/// Represents a response for resizing an image using TinyPNG API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TinyPngResizeResponse"/> class.
/// </remarks>
/// <param name="msg">The HTTP response message.</param>
public class TinyPngResizeResponse(HttpResponseMessage msg) : TinyPngImageResponse(msg)
{
}
