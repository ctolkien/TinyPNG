using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TinyPng.Responses;

/// <summary>
/// Represents the response from the TinyPNG API.
/// </summary>
public class TinyPngResponse
{
    internal HttpResponseMessage HttpResponseMessage { get; }

    private readonly int _compressionCount;

    /// <summary>
    /// Gets the number of compressions performed.
    /// </summary>
    public int CompressionCount => _compressionCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TinyPngResponse"/> class.
    /// </summary>
    /// <param name="msg">The HTTP response message.</param>
    protected TinyPngResponse(HttpResponseMessage msg)
    {
        if (msg is null)
        {
            throw new System.ArgumentNullException(nameof(msg));
        }
        if (msg.Headers.TryGetValues("Compression-Count", out IEnumerable<string> compressionCountHeaders))
        {
            _ = int.TryParse(compressionCountHeaders.First(), out _compressionCount);
        }
        HttpResponseMessage = msg;
    }
}
