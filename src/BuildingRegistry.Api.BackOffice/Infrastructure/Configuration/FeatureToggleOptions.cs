namespace BuildingRegistry.Api.BackOffice.Infrastructure.Configuration
{
    public sealed class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseSqs { get; set; }
    }
}
