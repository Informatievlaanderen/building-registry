namespace BuildingRegistry.Api.Grb.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.Runtime.Internal.Auth;
    using Amazon.S3;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// An extended Amazon S3 client, with added support for creating presigned posts.
    /// </summary>
    /// <seealso href="https://github.com/aws/aws-sdk-net/issues/1901">
    /// AmazonS3Client is missing Create Presigned Post
    /// </seealso>
    public sealed class AmazonS3ExtendedClient : AmazonS3Client, IAmazonS3Extended
    {
        private readonly ILogger<AmazonS3ExtendedClient> _logger;

        public AmazonS3ExtendedClient(
            ILoggerFactory loggerFactory,
            AWSCredentials credentials,
            AmazonS3ExtendedConfig config)
            : base(credentials, config)
        {
            _logger = loggerFactory.CreateLogger<AmazonS3ExtendedClient>();
        }

        public CreatePresignedPostResponse CreatePresignedPost(CreatePresignedPostRequest request)
        {
            var regionName = Config.RegionEndpoint.SystemName ??  RegionEndpoint.EUWest1.SystemName;

            _logger.LogWarning($"region: {regionName}");

            var url = new Uri($"https://s3.{regionName}.amazonaws.com/{request.BucketName}");

            _logger.LogWarning($"bucketurl: {url}");

            var signingDate = Config.CorrectedUtcNow
                .ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);

            var shortDate = signingDate[..8];

            var credentials = Credentials.GetCredentials();

            if (credentials is null)
            {
                _logger.LogError("No credentials found");
            }

            _logger.LogWarning($"credentials: {credentials.AccessKey}");
            _logger.LogWarning($"usetoken: {credentials.UseToken}");

            var credentialScope = $"{shortDate}/{regionName}/s3/aws4_request";

            var fields = new Dictionary<string, string>
            {
                { "key", request.Key },
                { "bucket", request.BucketName },
                { "X-Amz-Algorithm", "AWS4-HMAC-SHA256" },
                { "X-Amz-Credential", $"{credentials.AccessKey}/{credentialScope}" },
                { "X-Amz-Date", signingDate },
                { "X-Amz-Security-Token", credentials.Token }
            };

            foreach (var (key, value) in fields)
            {
                request.Conditions.Add(new ExactMatchCondition(key, value));
            }

            var postPolicy = new PostPolicy(
                Config.CorrectedUtcNow.Add(request.Expires ?? TimeSpan.FromSeconds(3600)),
                request.Conditions);

            var postPolicyJson = JsonSerializer.Serialize(
                postPolicy,
                AmazonS3SerializerContext.Default.PostPolicy);

            var postPolicyEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(postPolicyJson));

            fields.Add("Policy", postPolicyEncoded);

            var signingKey = AWS4Signer.ComposeSigningKey(
                credentials.SecretKey,
                regionName,
                shortDate,
                "s3");

            var signature =
                AWS4Signer.ComputeKeyedHash(SigningAlgorithm.HmacSHA256, signingKey, postPolicyEncoded);

            fields.Add("X-Amz-Signature", Convert.ToHexString(signature).ToLowerInvariant());

            return new CreatePresignedPostResponse(url, fields);
        }
    }

    public interface IAmazonS3Extended : IAmazonS3
    {
        public CreatePresignedPostResponse CreatePresignedPost(CreatePresignedPostRequest request);
    }

    public sealed class AmazonS3ExtendedConfig : AmazonS3Config
    {
    }

    public sealed record CreatePresignedPostRequest(
        string BucketName,
        string Key,
        IList<ExactMatchCondition> Conditions,
        TimeSpan? Expires);

    public sealed record CreatePresignedPostResponse(Uri Url, Dictionary<string, string> Fields);

    public sealed record PostPolicy(DateTime Expiration, IList<ExactMatchCondition> Conditions);

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(PostPolicy))]
    public sealed partial class AmazonS3SerializerContext : JsonSerializerContext
    {
    }

    // public abstract record Condition
    // {
    // }

    [JsonConverter(typeof(ExactMatchConditionConverter))]
    public sealed record ExactMatchCondition(string Key, string Value); //: Condition;

    // [JsonConverter(typeof(StartsWithMatchConditionConverter))]
    // public sealed record StartsWithMatchCondition(string Key, string Value) : Condition;
    //
    // [JsonConverter(typeof(RangeMatchConditionConverter))]
    // public sealed record RangeMatchCondition(string Key, int ValueStart, int ValueEnd) : Condition;

    public sealed class ExactMatchConditionConverter : JsonConverter<ExactMatchCondition>
    {
        public override ExactMatchCondition? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            ExactMatchCondition value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(value.Key, value.Value);
            writer.WriteEndObject();
        }
    }
    //
    // public sealed class StartsWithMatchConditionConverter : JsonConverter<StartsWithMatchCondition>
    // {
    //     public override StartsWithMatchCondition? Read(
    //         ref Utf8JsonReader reader,
    //         Type typeToConvert,
    //         JsonSerializerOptions options)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override void Write(
    //         Utf8JsonWriter writer,
    //         StartsWithMatchCondition value,
    //         JsonSerializerOptions options)
    //     {
    //         writer.WriteStartArray();
    //         writer.WriteStringValue("starts-with");
    //         writer.WriteStringValue(value.Key);
    //         writer.WriteStringValue(value.Value);
    //         writer.WriteEndArray();
    //     }
    // }
    //
    // public sealed class RangeMatchConditionConverter : JsonConverter<RangeMatchCondition>
    // {
    //     public override RangeMatchCondition? Read(
    //         ref Utf8JsonReader reader,
    //         Type typeToConvert,
    //         JsonSerializerOptions options)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override void Write(
    //         Utf8JsonWriter writer,
    //         RangeMatchCondition value,
    //         JsonSerializerOptions options)
    //     {
    //         writer.WriteStartArray();
    //         writer.WriteStringValue(value.Key);
    //         writer.WriteNumberValue(value.ValueStart);
    //         writer.WriteNumberValue(value.ValueEnd);
    //         writer.WriteEndArray();
    //     }
    // }
}
