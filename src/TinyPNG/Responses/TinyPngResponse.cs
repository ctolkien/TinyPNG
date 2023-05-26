using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TinyPng.Responses;

public class TinyPngResponse
{
    internal HttpResponseMessage HttpResponseMessage { get; }

    private readonly int compressionCount;

    public int CompressionCount => compressionCount;

    

    protected TinyPngResponse(HttpResponseMessage msg)
    {
        if (msg.Headers.TryGetValues("Compression-Count", out IEnumerable<string> compressionCountHeaders))
        {
            int.TryParse(compressionCountHeaders.First(), out compressionCount);
        }
        HttpResponseMessage = msg;
    }
}
