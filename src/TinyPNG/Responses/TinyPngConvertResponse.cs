using System.Linq;
using System.Net.Http;

namespace TinyPng.Responses;
public class TinyPngConvertResponse : TinyPngImageResponse
{
    public TinyPngConvertResponse(HttpResponseMessage msg) : base(msg)
    {
    }

    public string ContentType => HttpResponseMessage.Content.Headers.ContentType.MediaType;
    public string ImageHeight => HttpResponseMessage.Content.Headers.GetValues("Image-Height").FirstOrDefault();
    public string ImageWidth => HttpResponseMessage.Content.Headers.GetValues("Image-Width").FirstOrDefault();
}
