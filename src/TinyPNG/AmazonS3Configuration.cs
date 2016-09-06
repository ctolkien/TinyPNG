using System;
using Newtonsoft.Json;
namespace TinyPng
{
    public class AmazonS3Configuration 
    {
        [JsonProperty("service")]
        public const string Service = "s3";

        public AmazonS3Configuration(string awsAccessKeyId, string awsSecretAccessKey, string defaultRegion)
        {
            AwsAccessKeyId = awsAccessKeyId;
            AwsSecretAccessKey = awsSecretAccessKey;
            Region = defaultRegion;
        }

        [JsonProperty("aws_access_key_id")]
        public string AwsAccessKeyId { get;  }
        [JsonProperty("aws_secret_access_key")]
        public string AwsSecretAccessKey { get;  }

        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }

        public AmazonS3Configuration Clone()
        {
            return new AmazonS3Configuration(AwsAccessKeyId, AwsSecretAccessKey, Region);
        }

    }
}
