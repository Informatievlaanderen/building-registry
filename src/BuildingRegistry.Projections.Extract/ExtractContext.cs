namespace BuildingRegistry.Projections.Extract
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Building;
    using BuildingExtract;
    using BuildingUnitAddressLinkExtract;
    using BuildingUnitExtract;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<BuildingExtractItem> BuildingExtract { get; set; }
        public DbSet<BuildingExtractItemV2> BuildingExtractV2 { get; set; }
        public DbSet<BuildingUnitExtractItem> BuildingUnitExtract { get; set; }
        public DbSet<BuildingUnitExtractItemV2> BuildingUnitExtractV2 { get; set; }
        public DbSet<BuildingUnitBuildingItem> BuildingUnitBuildings { get; set; }
        public DbSet<BuildingUnitAddressLinkExtractItem> BuildingUnitAddressLinkExtract { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }

        public async Task<BuildingUnitAddressLinkExtractItem?> FindBuildingUnitAddressExtractItem(
            int buildingUnitPersistentLocalId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            return await BuildingUnitAddressLinkExtract.FindAsync(new object?[] {buildingUnitPersistentLocalId, addressPersistentLocalId}, ct);
        }
    }
}
