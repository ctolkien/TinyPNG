using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinyPngApi
{
    public class TinyPng : IDisposable
    {
        private readonly string _apiKey;
        private const string ApiEndpoint = "https://api.tinify.com/shrink";
        private HttpClient httpClient = new HttpClient();
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// Wrapper for the tinypng.com API
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        public TinyPng(string apiKey) 
        {
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            var auth = $"api:{apiKey}";
            var authByteArray = System.Text.Encoding.ASCII.GetBytes(auth);

            _apiKey = Convert.ToBase64String(authByteArray);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", _apiKey);

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        }


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
        /// <returns></returns>
        public async Task<TinyPngApiResult> Compress(string pathToFile)
        {
            if (pathToFile == null)
                throw new ArgumentNullException(nameof(pathToFile));

            return await Compress(File.ReadAllBytes(pathToFile));
        }

        /// <summary>
        /// Compress byte array of image
        /// </summary>
        /// <param name="data">Byte array of the data to compress</param>
        /// <returns></returns>
        public async Task<TinyPngApiResult> Compress(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var response = await httpClient.PostAsync(ApiEndpoint, CreateContent(data));

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TinyPngApiResult>(await response.Content.ReadAsStringAsync(), jsonSettings);
            }
            throw new Exception($"Api Service returned a non-success status code when attempting to compress an image: {response.StatusCode}");
        }

        /// <summary>
        /// Compress stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<TinyPngApiResult> Compress(Stream data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var response = await httpClient.PostAsync(ApiEndpoint, CreateContent(data));

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TinyPngApiResult>(await response.Content.ReadAsStringAsync(), jsonSettings);
            }
            throw new Exception($"Api Service returned a non-success status code when attempting to compress an image: {response.StatusCode}");
        }

        public async Task<object> Download(TinyPngApiResult response)
        {
            return await DownloadStream(response.Output.Url);
        }

        public async Task<Stream> DownloadStream(string url)
        {
            var streamOfBytes = await httpClient.GetStreamAsync(url);
            return streamOfBytes;
        }

        public async Task<Stream> DownloadBytes(string url)
        {
            var streamOfBytes = await httpClient.GetStreamAsync(url);
            return streamOfBytes;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    httpClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TinyPng()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    

    public static class Extensions
    {
        public async static Task<byte[]> ImageByteData(this TinyPngApiResult result)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(result.Output.Url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                throw new Exception($"Api Service returned a non-success status code when attempting to access a compressed image: {response.StatusCode}");
            }

        }

        /// <summary>
        /// Gets the image data as a stream
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Stream of compressed image data</returns>
        public async static Task<Stream> ImageStreamData(this TinyPngApiResult result)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(result.Output.Url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync();
                }
                throw new Exception($"Api Service returned a non-success status code when attempting to access a compressed image: {response.StatusCode}");
            }

        }

        /// <summary>
        /// Writes the image to disk
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <param name="filePath">The path to store the file</param>
        /// <returns></returns>
        public async static Task SaveImage(this TinyPngApiResult result, string filePath)
        {
            var byteData = await result.ImageByteData();
            File.WriteAllBytes(filePath, byteData);
        }


    }
}
