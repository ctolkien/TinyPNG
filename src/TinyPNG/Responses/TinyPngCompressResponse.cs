using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TinyPng.Responses;

public class TinyPngCompressResponse : TinyPngResponse
{
    public TinyPngApiInput Input { get; private set; }
    public TinyPngApiOutput Output { get; private set; }
    public TinyPngApiResult ApiResult { get; private set; }

    internal readonly HttpClient HttpClient;

    public TinyPngCompressResponse(HttpResponseMessage msg, HttpClient httpClient) : base(msg)
    {
        HttpClient = httpClient;

        //this is a cute trick to handle async in a ctor and avoid deadlocks
        ApiResult = Task.Run(() => Deserialize(msg)).GetAwaiter().GetResult();
        Input = ApiResult.Input;
        Output = ApiResult.Output;

    }
    private async Task<TinyPngApiResult> Deserialize(HttpResponseMessage response)
    {
        return await JsonSerializer.DeserializeAsync<TinyPngApiResult>(await response.Content.ReadAsStreamAsync(), TinyPngClient.JsonOptions);
    }
}
