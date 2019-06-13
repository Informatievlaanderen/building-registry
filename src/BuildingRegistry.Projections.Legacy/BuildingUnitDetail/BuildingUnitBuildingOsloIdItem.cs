namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetail
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitBuildingOsloIdItem
    {
        public Guid BuildingId { get; set; }
        public int? BuildingOsloId { get; set; }
    }

    public class BuildingUnitBuildingOsloIdItemConfiguration : IEntityTypeConfiguration<BuildingUnitBuildingOsloIdItem>
    {
        public const string TableName = "BuildingUnit_BuildingOsloIds";

        public void Configure(EntityTypeBuilder<BuildingUnitBuildingOsloIdItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingId)
                .ForSqlServerIsClustered(false);

            b.HasIndex(p => p.BuildingOsloId);
        }
    }
}
