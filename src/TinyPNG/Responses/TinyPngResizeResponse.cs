using System.Net.Http;

namespace TinyPng.Responses;

public class TinyPngResizeResponse(HttpResponseMessage msg) : TinyPngImageResponse(msg)
{
}
