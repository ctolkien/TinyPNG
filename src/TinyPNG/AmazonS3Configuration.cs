using System.Text.Json.Serialization;

namespace TinyPng;


/// <summary>
/// Represents the configuration for Amazon S3.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AmazonS3Configuration"/> class.
/// </remarks>
/// <param name="awsAccessKeyId">The AWS access key ID.</param>
/// <param name="awsSecretAccessKey">The AWS secret access key.</param>
/// <param name="defaultBucket">The default bucket name.</param>
/// <param name="defaultRegion">The default region.</param>
public sealed class AmazonS3Configuration(string awsAccessKeyId, string awsSecretAccessKey, string defaultBucket, string defaultRegion)
{
    /// <summary>
    /// The service name.
    /// </summary>
    [JsonPropertyName("service")]
    public const string Service = "s3";

    /// <summary>
    /// The AWS access key ID.
    /// </summary>
    [JsonPropertyName("aws_access_key_id")]
    public string AwsAccessKeyId { get; } = awsAccessKeyId;

    /// <summary>
    /// The AWS secret access key.
    /// </summary>
    [JsonPropertyName("aws_secret_access_key")]
    public string AwsSecretAccessKey { get; } = awsSecretAccessKey;

    /// <summary>
    /// The region.
    /// </summary>
    public string Region { get; set; } = defaultRegion;

    /// <summary>
    /// The bucket name.
    /// </summary>
    [JsonIgnore]
    public string Bucket { get; set; } = defaultBucket;

    /// <summary>
    /// The path within the bucket.
    /// </summary>
    [JsonIgnore]
    public string Path { get; set; }

    /// <summary>
    /// The bucket path.
    /// </summary>
    [JsonPropertyName("path")]
    public string BucketPath => $"{Bucket}/{Path}";

    public AmazonS3Configuration Clone()
    {
        return new AmazonS3Configuration(AwsAccessKeyId, AwsSecretAccessKey, Bucket, Region)
        {
            Path = Path
        };
    }
}
