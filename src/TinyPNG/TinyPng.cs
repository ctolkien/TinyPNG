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

        public HttpClient httpClient = new HttpClient();
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
            jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

        }

        /// <summary>
        /// Wrapper for the tinypng.com API
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        /// <param name="amazonConfiguration">Configures defaults to use for storing images on Amazon S3</param>
        public TinyPngClient(string apiKey, AmazonS3Configuration amazonConfiguration) : this(apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            if (amazonConfiguration == null)
                throw new ArgumentNullException(nameof(amazonConfiguration));

            AmazonS3Configuration = amazonConfiguration;
        }

        /// <summary>
        /// Configures the client to use these AmazonS3 settings when storing images in S3
        /// </summary>
        public AmazonS3Configuration AmazonS3Configuration { get; set; }

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
        public async Task<TinyPngImageResponse> Download(TinyPngCompressResponse result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var msg = new HttpRequestMessage(HttpMethod.Get, result.Output.Url);

            var response = await httpClient.SendAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                return new TinyPngImageResponse(response);
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
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (resizeOperation == null)
                throw new ArgumentNullException(nameof(resizeOperation));

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
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (width == 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height == 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var resizeOp = new ResizeOperation(resizeType, width, height);

            return await Resize(result, height, width, resizeType);
        }


        /// <summary>
        /// Stores a previously compressed image directly into Amazon S3 storage
        /// </summary>
        /// <param name="result">The previously compressed image</param>
        /// <param name="amazonSettings">The settings for the amazon connection</param>
        /// <param name="path">The path and bucket to store in: bucket/file.png format</param>
        /// <returns></returns>
        public async Task<Uri> SaveCompressedImageToAmazonS3(TinyPngCompressResponse result, AmazonS3Configuration amazonSettings, string path)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (amazonSettings == null)
                throw new ArgumentNullException(nameof(amazonSettings));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            amazonSettings.Path = path;

            var amazonSettingsAsJson = JsonConvert.SerializeObject(new { store = amazonSettings }, jsonSettings);

            var msg = new HttpRequestMessage(HttpMethod.Post, result.Output.Url);
            msg.Content = new StringContent(amazonSettingsAsJson, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                return response.Headers.Location;
            }

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync());
            throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
        }

        /// <summary>
        /// Stores a previously compressed image directly into Amazon S3 storage
        /// </summary>
        /// <param name="result">The previously compressed image</param>
        /// <param name="path">The path to storage the image as</param>
        /// <param name="bucketOverride">Optional: To override the previously configured bucket</param>
        /// <param name="regionOverride">Optional: To override the previously configured region</param>
        /// <returns></returns>
        public async Task<Uri> SaveCompressedImageToAmazonS3(TinyPngCompressResponse result, string path, string bucketOverride = "", string regionOverride = "")
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (AmazonS3Configuration == null)
                throw new InvalidOperationException("AmazonS3Configuration has not been configured");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var amazonSettings = AmazonS3Configuration.Clone();
            amazonSettings.Path = path;

            if (!string.IsNullOrEmpty(regionOverride))
                amazonSettings.Region = regionOverride;

            if (!string.IsNullOrEmpty(bucketOverride))
                amazonSettings.Bucket = bucketOverride;

            return await SaveCompressedImageToAmazonS3(result, amazonSettings, path);
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
