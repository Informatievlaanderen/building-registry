namespace BuildingRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingExtract;
    using BuildingUnitExtract;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using BuildingUnitAddressLinkExtractItemWithCount = BuildingUnitAddressLinkExtractWithCount.BuildingUnitAddressLinkExtractItem;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<BuildingExtractItemV2> BuildingExtractV2 { get; set; }
        public DbSet<BuildingExtractItemV2Esri> BuildingExtractV2Esri { get; set; }
        public DbSet<BuildingUnitExtractItemV2> BuildingUnitExtractV2 { get; set; }
        public DbSet<BuildingUnitAddressLinkExtractItemWithCount> BuildingUnitAddressLinkExtractWithCount { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }
    }
}
