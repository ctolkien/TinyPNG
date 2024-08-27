using System.Text.Json.Serialization;

namespace TinyPng.Responses;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TinyPngApiResult))]
[JsonSerializable(typeof(ApiErrorResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
