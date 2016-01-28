using System.Net.Http;

namespace TinyPng.Responses
{
    public class TinyPngResizeResponse : TinyPngResponse
    {
        public TinyPngResizeResponse(HttpResponseMessage msg) : base(msg)
        {

        }
    }
}
