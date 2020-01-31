namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
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
        private const string TableName = "BuildingUnit_Buildings";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingItem> b)
        {
            b.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.BuildingId)
                .IsClustered(false);

            b.Property(p => p.IsComplete);
            b.Property(p => p.IsRemoved);
            b.Property(p => p.BuildingPersistentLocalId);
            b.Property(p => p.BuildingRetiredStatus);

            b.HasIndex(p => p.BuildingPersistentLocalId);
        }
    }
}
