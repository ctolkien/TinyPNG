using System.IO;
using System.Threading.Tasks;
using TinyPng.Responses;

namespace TinyPng
{
    public static class ImageDataExtensions
    {
        /// <summary>
        /// Get the image data as a byte array
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Byte array of the image data</returns>
        public static async Task<byte[]> GetImageByteDataAsync<T>(this Task<T> result) where T: TinyPngImageResponse
        {
            var imageResponse = await result;
            return await GetImageByteDataAsync(imageResponse);
        }

        /// <summary>
        /// Get the image data as a byte array
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Byte array of the image data</returns>
        public static async Task<byte[]> GetImageByteDataAsync(this TinyPngImageResponse result)
        {
            return await result.HttpResponseMessage.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Gets the image data as a stream
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Stream of compressed image data</returns>
        public static async Task<Stream> GetImageStreamDataAsync<T>(this Task<T> result) where T : TinyPngImageResponse
        {
            var imageResponse = await result;
            return await GetImageStreamDataAsync(imageResponse);
        }

        /// <summary>
        /// Gets the image data as a stream
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Stream of compressed image data</returns>
        public static async Task<Stream> GetImageStreamDataAsync(this TinyPngImageResponse result)
        {
            return await result.HttpResponseMessage.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Writes the image to disk
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <param name="filePath">The path to store the file</param>
        /// <returns></returns>
        public static async Task SaveImageToDiskAsync<T>(this Task<T> result, string filePath) where T : TinyPngImageResponse
        {
            var response = await result;
            await SaveImageToDiskAsync(response, filePath);
        }

        /// <summary>
        /// Writes the image to disk
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <param name="filePath">The path to store the file</param>
        /// <returns></returns>
        public static async Task SaveImageToDiskAsync(this TinyPngImageResponse result, string filePath)
        {
            var byteData = await result.GetImageByteDataAsync();
            File.WriteAllBytes(filePath, byteData);
        }
    }
}
