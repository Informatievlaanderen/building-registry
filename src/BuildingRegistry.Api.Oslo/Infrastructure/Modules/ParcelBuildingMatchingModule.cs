namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Consumer.Read.Parcel;
    using Microsoft.Extensions.Configuration;
    using Projections.Legacy;

    public class ParcelBuildingMatchingModule : Module
    {
        private readonly IConfiguration _configuration;

        public ParcelBuildingMatchingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            // Both matching implementations resolve the reference system to compare in from this toggle.
            // A call site that resolved it from anywhere else would compare in a system the column is not
            // in, and fail silently in both of the ways ADR 0006 describes.
            builder
                .RegisterInstance(new Lambert2008ConversionCompletedToggle(
                    _configuration.GetValue<bool>("FeatureToggles:Lambert2008ConversionCompleted")))
                .SingleInstance();

            builder
                .RegisterInstance(new Lambert2008MatchingReadiness())
                .SingleInstance();

            builder
                .RegisterType<ParcelMatching>()
                .AsImplementedInterfaces();

            builder
                .RegisterType<BuildingMatching>()
                .AsImplementedInterfaces();
        }
    }
}
