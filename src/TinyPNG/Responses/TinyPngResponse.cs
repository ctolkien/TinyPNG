using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TinyPng.Responses;

public class TinyPngResponse
{
    internal HttpResponseMessage HttpResponseMessage { get; }

    private readonly int _compressionCount;

    public int CompressionCount => _compressionCount;

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
