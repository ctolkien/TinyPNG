using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TinyPng.Responses
{
    public class TinyPngCompressResponse : TinyPngResponse
    {
        public TinyPngApiInput Input { get; private set; }
        public TinyPngApiOutput Output { get; private set; }
        public TinyPngApiResult ApiResult { get; private set; }

        [Obsolete]
        private readonly JsonSerializerSettings jsonSettings;
        internal readonly HttpClient HttpClient;

        public TinyPngCompressResponse(HttpResponseMessage msg, HttpClient httpClient) : base(msg)
        {
            HttpClient = httpClient;
            //configure json settings for camelCase.
            jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            //this is a cute trick to handle async in a ctor and avoid deadlocks
            ApiResult = Task.Run(() => Deserialize(msg)).GetAwaiter().GetResult();
            Input = ApiResult.Input;
            Output = ApiResult.Output;

        }
        private async Task<TinyPngApiResult> Deserialize(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<TinyPngApiResult>(await response.Content.ReadAsStringAsync(), jsonSettings);
        }
    }
}
