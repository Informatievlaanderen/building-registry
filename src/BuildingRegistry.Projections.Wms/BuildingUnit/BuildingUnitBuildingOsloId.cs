namespace BuildingRegistry.Projections.Wms.BuildingUnit
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitBuildingOsloId
    {
        public Guid BuildingId { get; set; }
        public string BuildingOsloId { get; set; }
    }

    public class BuildingUnitBuildingOsloIdConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingOsloId>
    {
        public const string TableName = "BuildingUnit_BuildingOsloIds";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingOsloId> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.BuildingId)
                .ForSqlServerIsClustered(false);

            b.HasIndex(p => p.BuildingOsloId);
        }
    }
}
