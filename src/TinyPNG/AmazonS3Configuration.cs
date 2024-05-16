using System.Text.Json.Serialization;

namespace TinyPng
{
    public class AmazonS3Configuration(string awsAccessKeyId,
        string awsSecretAccessKey,
        string defaultBucket,
        string defaultRegion)
    {
        [JsonPropertyName("service")]
        public const string Service = "s3";

        [JsonPropertyName("aws_access_key_id")]
        public string AwsAccessKeyId { get; } = awsAccessKeyId;
        [JsonPropertyName("aws_secret_access_key")]
        public string AwsSecretAccessKey { get; } = awsSecretAccessKey;
        public string Region { get; set; } = defaultRegion;
        [JsonIgnore]
        public string Bucket { get; set; } = defaultBucket;
        [JsonIgnore]
        public string Path { get; set; }

        [JsonPropertyName("path")]
        public string BucketPath
        {
            get
            {
                return $"{Bucket}/{Path}";
            }
        }

        public AmazonS3Configuration Clone()
        {
            return new AmazonS3Configuration(AwsAccessKeyId, AwsSecretAccessKey, Bucket, Region);
        }

    }
}
