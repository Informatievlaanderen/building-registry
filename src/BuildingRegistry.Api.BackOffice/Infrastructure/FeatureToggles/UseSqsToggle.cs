namespace BuildingRegistry.Api.BackOffice.Infrastructure.FeatureToggles
{
    using FeatureToggle;

    public sealed class UseSqsToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseSqsToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
