namespace BuildingRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingExtract;
    using BuildingUnitExtract;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<BuildingExtractItem> BuildingExtract { get; set; }
        public DbSet<BuildingUnitExtractItem> BuildingUnitExtract { get; set; }
        public DbSet<BuildingUnitBuildingItem> BuildingUnitBuildings { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }
    }
}
