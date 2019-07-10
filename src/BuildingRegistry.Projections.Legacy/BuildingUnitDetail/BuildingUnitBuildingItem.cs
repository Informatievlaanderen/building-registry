namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetail
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ValueObjects;

    public class BuildingUnitBuildingItem
    {
        public Guid BuildingId { get; set; }
        public int? BuildingPersistentLocalId { get; set; }
        public bool? IsComplete { get; set; }
        public bool IsRemoved { get; set; }
        public BuildingStatus? BuildingRetiredStatus { get; set; }
    }

    public class BuildingUnitBuildingPersistentLocalIdItemConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingItem>
    {
        public const string TableName = "BuildingUnit_Buildings";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.IsComplete);
            b.Property(p => p.IsRemoved);
            b.Property(p => p.BuildingPersistentLocalId);
            b.Property(p => p.BuildingRetiredStatus);

            b.HasIndex(p => p.BuildingPersistentLocalId);
        }
    }
}
