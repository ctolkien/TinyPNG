﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TinyPng.Responses;

[assembly:InternalsVisibleTo("TinyPng.Tests")]
namespace TinyPng
{
    public class TinyPngClient
    {
        private const string ApiEndpoint = "https://api.tinify.com/shrink";

        private readonly HttpClient HttpClient;
        internal static readonly JsonSerializerSettings JsonSettings;

        /// <summary>
        /// Configures the client to use these AmazonS3 settings when storing images in S3
        /// </summary>
        public AmazonS3Configuration AmazonS3Configuration { get; set; }
        
        static TinyPngClient()
        {
            //configure json settings for camelCase.
            JsonSettings = new JsonSerializerSettings
                           {
                               ContractResolver = new CamelCasePropertyNamesContractResolver(),
                               NullValueHandling = NullValueHandling.Ignore
                           };
            JsonSettings.Converters.Add(new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()});
        }
        
        /// <summary>
        /// Wrapper for the tinypng.com API
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        /// <param name="httpClient">HttpClient for requests (optional) </param>
        public TinyPngClient(string apiKey, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            HttpClient = httpClient ?? new HttpClient();
            
            ConfigureHttpClient(apiKey);
        }

        private void ConfigureHttpClient(string apiKey)
        {
            //configure basic auth api key formatting.
            var auth = $"api:{apiKey}";
            var authByteArray = Encoding.ASCII.GetBytes(auth);
            var apiKeyEncoded = Convert.ToBase64String(authByteArray);

            //add auth to the default outgoing headers.
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", apiKeyEncoded);
        }

        /// <summary>
        /// Wrapper for the tinypng.com API
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        /// <param name="amazonConfiguration">Configures defaults to use for storing images on Amazon S3</param>
        /// <param name="httpClient">HttpClient for requests (optional) </param>
        public TinyPngClient(string apiKey, AmazonS3Configuration amazonConfiguration, HttpClient httpClient = null)
            : this(apiKey, httpClient)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));
            
            AmazonS3Configuration = amazonConfiguration ?? throw new ArgumentNullException(nameof(amazonConfiguration));
        }

        private static HttpContent CreateContent(byte[] source) => new ByteArrayContent(source);

        private static HttpContent CreateContent(Stream source) => new StreamContent(source);

        private static HttpContent CreateContent(string source) => new StringContent(
            JsonConvert.SerializeObject(new {source = new {url = source}}, JsonSettings),
            Encoding.UTF8, "application/json");

        /// <summary>
        /// Compress a file on disk
        /// </summary>
        /// <param name="pathToFile">Path to file on disk</param>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public async Task<TinyPngCompressResponse> Compress(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile))
                throw new ArgumentNullException(nameof(pathToFile));

            using (var file = File.OpenRead(pathToFile))
            {
                return await Compress(file).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Compress byte array of image
        /// </summary>
        /// <param name="data">Byte array of the data to compress</param>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public async Task<TinyPngCompressResponse> Compress(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var stream = new MemoryStream(data))
            {
                return await Compress(stream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Compress a stream
        /// </summary>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public Task<TinyPngCompressResponse> Compress(Stream data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            return CompressInternal(CreateContent(data));
        }

        /// <summary>
        /// Compress image from url
        /// </summary>
        /// <param name="url">Image url to compress</param>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public Task<TinyPngCompressResponse> CompressFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            
            return CompressInternal(CreateContent(url));
        }

        private async Task<TinyPngCompressResponse> CompressInternal(HttpContent contentData)
        {
            var response = await HttpClient.PostAsync(ApiEndpoint, contentData).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return new TinyPngCompressResponse(response, HttpClient);

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
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

            var amazonSettingsAsJson = JsonConvert.SerializeObject(new { store = amazonSettings }, JsonSettings);

            var msg = new HttpRequestMessage(HttpMethod.Post, result.Output.Url)
            {
                Content = new StringContent(amazonSettingsAsJson, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await HttpClient.SendAsync(msg).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return response.Headers.Location;
            }

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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
        public Task<Uri> SaveCompressedImageToAmazonS3(TinyPngCompressResponse result, string path, string bucketOverride = "", string regionOverride = "")
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

            return SaveCompressedImageToAmazonS3(result, amazonSettings, path);
        }
    }
}
