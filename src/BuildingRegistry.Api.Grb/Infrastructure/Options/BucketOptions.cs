namespace BuildingRegistry.Api.Grb.Infrastructure.Options
{
    public sealed class BucketOptions
    {
        public const string ConfigKey = "Bucket";

        public string BucketName { get; set; }
        public int UrlExpirationInMinutes { get; set; }
    }
}
