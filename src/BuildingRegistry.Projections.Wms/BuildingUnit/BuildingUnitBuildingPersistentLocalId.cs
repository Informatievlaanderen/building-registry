namespace BuildingRegistry.Projections.Wms.BuildingUnit
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitBuildingPersistentLocalId
    {
        public Guid BuildingId { get; set; }
        public string? BuildingPersistentLocalId { get; set; }
    }

    public class BuildingUnitBuildingPersistentLocalIdConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingPersistentLocalId>
    {
        private const string TableName = "BuildingUnit_BuildingPersistentLocalIds";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingPersistentLocalId> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.BuildingId)
                .IsClustered(false);

            b.HasIndex(p => p.BuildingPersistentLocalId);
        }
    }
}
