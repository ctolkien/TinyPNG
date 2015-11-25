using System.Net.Http;

namespace TinyPngApi
{

    public class TinyPngHttpResponseMessage
    {
        public HttpResponseMessage ResponseMessage { get; private set; }

        public TinyPngHttpResponseMessage(HttpResponseMessage msg)
        {
            ResponseMessage = msg;
        }
    }

    public class TinyPngApiResult
    {
        public TinyPngApiInput Input { get; set; }
        public TinyPngApiOutput Output { get; set; }
    }

    public class TinyPngApiInput
    {
        public int Size { get; set; }
        public string Type { get; set; }
    }

    public class TinyPngApiOutput
    {
        public int Size { get; set; }
        public string Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Ratio { get; set; }
        public string Url { get; set; }
    }
}
