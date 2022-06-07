namespace BuildingRegistry.Projections.Wms.BuildingUnitV2
{
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitBuildingItemV2
    {
        public int BuildingPersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public BuildingStatus? BuildingRetiredStatus { get; set; }
    }

    public class BuildingUnitBuildingPersistentLocalIdItemConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingItemV2>
    {
        private const string TableName = "BuildingUnit_BuildingsV2";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingItemV2> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.BuildingPersistentLocalId)
                .IsClustered(true);

            b.Property(p => p.BuildingPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.IsRemoved);
            b.Property(p => p.BuildingRetiredStatus)
                .HasConversion(x => x.HasValue ? x.Value.Value : null,
                    y => string.IsNullOrEmpty(y) ? null : BuildingStatus.Parse(y));
        }
    }
}
