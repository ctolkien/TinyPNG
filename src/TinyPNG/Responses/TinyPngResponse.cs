using System.Net.Http;

namespace TinyPngApi
{
    public class TinyPngResponse
    {
        public HttpResponseMessage HttpResponseMessage { get; private set; }

        protected TinyPngResponse(HttpResponseMessage msg)
        {
            HttpResponseMessage = msg;

        }
    }
}
