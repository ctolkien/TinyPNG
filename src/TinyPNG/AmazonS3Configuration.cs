using System.Text.Json.Serialization;

namespace TinyPng
{
    public class AmazonS3Configuration
    {
        [JsonPropertyName("service")]
        public const string Service = "s3";

        public AmazonS3Configuration(string awsAccessKeyId,
            string awsSecretAccessKey,
            string defaultBucket,
            string defaultRegion)
        {
            AwsAccessKeyId = awsAccessKeyId;
            AwsSecretAccessKey = awsSecretAccessKey;
            Bucket = defaultBucket;
            Region = defaultRegion;
        }

        [JsonPropertyName("aws_access_key_id")]
        public string AwsAccessKeyId { get; }
        [JsonPropertyName("aws_secret_access_key")]
        public string AwsSecretAccessKey { get; }
        public string Region { get; set; }
        [JsonIgnore]
        public string Bucket { get; set; }
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
