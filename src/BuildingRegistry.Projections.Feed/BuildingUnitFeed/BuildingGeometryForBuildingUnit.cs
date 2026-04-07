namespace BuildingRegistry.Projections.Feed.BuildingUnitFeed
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingGeometryForBuildingUnit
    {
        public int BuildingPersistentLocalId { get; set; }
        public string ExtendedWkbGeometry { get; set; } = string.Empty;

        private BuildingGeometryForBuildingUnit() { }

        public BuildingGeometryForBuildingUnit(int buildingPersistentLocalId, string extendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            ExtendedWkbGeometry = extendedWkbGeometry;
        }
    }

    public class BuildingGeometryForBuildingUnitConfiguration : IEntityTypeConfiguration<BuildingGeometryForBuildingUnit>
    {
        public void Configure(EntityTypeBuilder<BuildingGeometryForBuildingUnit> b)
        {
            b.ToTable("BuildingGeometryForBuildingUnit", Schema.Feed)
                .HasKey(x => x.BuildingPersistentLocalId)
                .IsClustered();

            b.Property(x => x.BuildingPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.ExtendedWkbGeometry)
                .IsRequired();
        }
    }
}
