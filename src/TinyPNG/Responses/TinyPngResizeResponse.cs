using System.Net.Http;

namespace TinyPngApi.Responses
{
    public class TinyPngResizeResponse : TinyPngResponse
    {
        public TinyPngResizeResponse(HttpResponseMessage msg) : base(msg)
        {

        }
    }
}
