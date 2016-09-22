using System.IO;
using System.Threading.Tasks;
using TinyPng.Responses;

namespace TinyPng
{
    public static class Extensions
    {
        /// <summary>
        /// Get the image data as a byte array
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Byte array of the image data</returns>
        public async static Task<byte[]> GetImageByteData(this TinyPngImageResponse result)
        {
            return await result.HttpResponseMessage.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Gets the image data as a stream
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <returns>Stream of compressed image data</returns>
        public async static Task<Stream> GetImageStreamData(this TinyPngImageResponse result)
        {
            return await result.HttpResponseMessage.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Writes the image to disk
        /// </summary>
        /// <param name="result">The result from compress</param>
        /// <param name="filePath">The path to store the file</param>
        /// <returns></returns>
        public async static Task SaveImageToDisk(this TinyPngImageResponse result, string filePath)
        {
            var byteData = await result.GetImageByteData();
            File.WriteAllBytes(filePath, byteData);
        }


    }
}
