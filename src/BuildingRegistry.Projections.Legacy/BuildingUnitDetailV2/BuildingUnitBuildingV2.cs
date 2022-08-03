namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitBuildingItemV2
    {
        public int BuildingPersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public BuildingStatus? BuildingRetiredStatus { get; set; }

        private BuildingUnitBuildingItemV2()
        {}

        public BuildingUnitBuildingItemV2(
            int buildingPersistentLocalId,
            bool isRemoved,
            BuildingStatus? buildingRetiredStatus)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            IsRemoved = isRemoved;
            BuildingRetiredStatus = buildingRetiredStatus;
        }
    }

    public class BuildingUnitBuildingV2PersistentLocalIdItemConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingItemV2>
    {
        private const string TableName = "BuildingUnit_BuildingsV2";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingPersistentLocalId)
                .IsClustered(false);

            b.Property(p => p.BuildingPersistentLocalId).
                ValueGeneratedNever();
            b.Property(p => p.IsRemoved);
            b.Property(p => p.BuildingRetiredStatus)
                .HasConversion(
                    x => x == null ? null : x.Value.Value,
                    y => string.IsNullOrEmpty(y) ? null : BuildingStatus.Parse(y));
        }
    }
}
