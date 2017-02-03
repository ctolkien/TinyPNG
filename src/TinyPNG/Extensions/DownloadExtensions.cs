using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TinyPng.Responses;

namespace TinyPng
{

    public static class DownloadExtensions
    {
        /// <summary>
        /// Downloads the result of a TinyPng Compression operation
        /// </summary>
        /// <param name="compressResponse"></param>
        /// <returns></returns>
        public async static Task<TinyPngImageResponse> Download(this Task<TinyPngCompressResponse> compressResponse)
        {
            if (compressResponse == null)
                throw new ArgumentNullException(nameof(compressResponse));

            var compressResult = await compressResponse;

            return await Download(compressResult);
            
        }

        /// <summary>
        /// Downloads the result of a TinyPng Compression operation
        /// </summary>
        /// <param name="compressResponse"></param>
        /// <returns></returns>
        public async static Task<TinyPngImageResponse> Download(TinyPngCompressResponse compressResponse)
        {
            if (compressResponse == null)
                throw new ArgumentNullException(nameof(compressResponse));

            var msg = new HttpRequestMessage(HttpMethod.Get, compressResponse.Output.Url);

            var response = await compressResponse.HttpClient.SendAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                return new TinyPngImageResponse(response);
            }

            var errorMsg = JsonConvert.DeserializeObject<ApiErrorResponse>(await response.Content.ReadAsStringAsync());
            throw new TinyPngApiException((int)response.StatusCode, response.ReasonPhrase, errorMsg.Error, errorMsg.Message);
        }
    }
}
