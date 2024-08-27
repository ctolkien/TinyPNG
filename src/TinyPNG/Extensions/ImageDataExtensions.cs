using System;
using System.IO;
using System.Threading.Tasks;
using TinyPng.Responses;

namespace TinyPng;

public static class ImageDataExtensions
{
    /// <summary>
    /// Get the image data as a byte array
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <returns>Byte array of the image data</returns>
    public static async Task<byte[]> GetImageByteData<T>(this Task<T> result) where T : TinyPngImageResponse
    {
        T imageResponse = await result;
        return await imageResponse.GetImageByteData();
    }

    /// <summary>
    /// Get the image data as a byte array
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <returns>Byte array of the image data</returns>
    public static async Task<byte[]> GetImageByteData(this TinyPngImageResponse result)
    {
        return result is null
            ? throw new ArgumentNullException(nameof(result))
            : await result.HttpResponseMessage.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Gets the image data as a stream
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <returns>Stream of compressed image data</returns>
    public static async Task<Stream> GetImageStreamData<T>(this Task<T> result) where T : TinyPngImageResponse
    {
        T imageResponse = await result;
        return await imageResponse.GetImageStreamData();
    }

    /// <summary>
    /// Gets the image data as a stream
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <returns>Stream of compressed image data</returns>
    public static async Task<Stream> GetImageStreamData(this TinyPngImageResponse result)
    {
        return result is null
            ? throw new ArgumentNullException(nameof(result))
            : await result.HttpResponseMessage.Content.ReadAsStreamAsync();
    }

    /// <summary>
    /// Writes the image to disk
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <param name="filePath">The path to store the file</param>
    /// <returns></returns>
    public static async Task SaveImageToDisk<T>(this Task<T> result, string filePath) where T : TinyPngImageResponse
    {
        T response = await result;
        await SaveImageToDisk(response, filePath);
    }

    /// <summary>
    /// Writes the image to disk
    /// </summary>
    /// <param name="result">The result from compress</param>
    /// <param name="filePath">The path to store the file</param>
    /// <returns></returns>
    public static async Task SaveImageToDisk(this TinyPngImageResponse result, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }
        byte[] byteData = await result.GetImageByteData();
        File.WriteAllBytes(filePath, byteData);
    }
}
