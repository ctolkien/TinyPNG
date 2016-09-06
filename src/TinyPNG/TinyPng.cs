using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TinyPng.Responses;

namespace TinyPng
{
    public class TinyPngClient : IDisposable
    {
        private readonly string _apiKey;
        private const string ApiEndpoint = "https://api.tinify.com/shrink";
        private HttpClient httpClient = new HttpClient();
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Wrapper for the tinypng.com API
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        public TinyPngClient(string apiKey) 
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            //configure basic auth api key formatting.
            var auth = $"api:{apiKey}";
            var authByteArray = System.Text.Encoding.ASCII.GetBytes(auth);
            _apiKey = Convert.ToBase64String(authByteArray);

            //add auth to the default outgoing headers.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _apiKey);

            //configure json settings for camelCase.
            jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            jsonSettings.Converters.Add(new StringEnumConverter {CamelCaseText = true });
            
        }

        public int CompressionCount { get; private set; }

        private HttpContent CreateContent(byte[] source)
        {
            return new ByteArrayContent(source);
        }
        private HttpContent CreateContent(Stream source)
        {
            return new StreamContent(source);
        }

        /// <summary>
        /// Compress file
        /// </summary>
        /// <param name="pathToFile">Path to file on disk</param>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public async Task<TinyPngCompressResponse> Compress(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile))
                throw new ArgumentNullException(nameof(pathToFile));

            using (var file = File.OpenRead(pathToFile))
            {
                return await Compress(file);
            }
        }

        /// <summary>
        /// Compress byte array of image
        /// </summary>
        /// <param name="data">Byte array of the data to compress</param>
        /// <returns></returns>
        public async Task<TinyPngCompressResponse> Compress(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var stream = new MemoryStream(data))
            {
                return await Compress(stream);
            }
        }

        /// <summary>
        /// Compress stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<TinyPngCompressResponse> Compress(Stream data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var response = await httpClient.PostAsync(ApiEndpoint, CreateContent(data));

            if (response.IsSuccessStatusCode)
            {
                return new TinyPngCompressResponse(response);
            }

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync());
            throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
        }


        /// <summary>
        /// Uses the TinyPng API to create a resized version of your uploaded image.
        /// </summary>
        /// <param name="result">This is the previous result of running a compression <see cref="Compress(string)"/></param>
        /// <param name="resizeOperation">Supply a strongly typed Resize Operation. See <typeparamref name="CoverResizeOperation"/>, <typeparamref name="FitResizeOperation"/>, <typeparamref name="ScaleHeightResizeOperation"/>, <typeparamref name="ScaleWidthResizeOperation"/></param>
        /// <returns></returns>
        public async Task<TinyPngResizeResponse> Resize(TinyPngCompressResponse result, ResizeOperation resizeOperation)
        {

            var requestBody = JsonConvert.SerializeObject(new { resize = resizeOperation }, jsonSettings);


            var msg = new HttpRequestMessage(HttpMethod.Post, result.Output.Url);
            msg.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                return new TinyPngResizeResponse(response);
            }

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync());
            throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
        }

        /// <summary>
        /// Uses the TinyPng API to create a resized version of your uploaded image.
        /// </summary>
        /// <param name="result">This is the previous result of running a compression <see cref="Compress(string)"/></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="resizeType"></param>
        /// <returns></returns>
        public async Task<TinyPngResizeResponse> Resize(TinyPngCompressResponse result, int width, int height, ResizeType resizeType = ResizeType.Fit)
        {
            var resizeOp = new ResizeOperation(resizeType, width, height);

            return await Resize(result, height, width, resizeType);

        }
        #region IDisposable Support
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient?.Dispose();
            }
        }
        #endregion
    }
    
}
