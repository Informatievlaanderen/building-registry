namespace BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitAddressSyndicationItemV2
    {
        public long Position { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        private BuildingUnitAddressSyndicationItemV2()
        { }

        public BuildingUnitAddressSyndicationItemV2(long position, int buildingUnitPersistentLocalId, int addressPersistentLocalId)
        {
            Position = position;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Count = 1;
        }

        public BuildingUnitAddressSyndicationItemV2 CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitAddressSyndicationItemV2
            {
                Position = position,
                BuildingUnitPersistentLocalId = BuildingUnitPersistentLocalId,
                AddressPersistentLocalId = AddressPersistentLocalId,
                Count = Count
            };

            return newItem;
        }
    }

    public class BuildingUnitAddressSyndicationItemV2Configuration : IEntityTypeConfiguration<BuildingUnitAddressSyndicationItemV2>
    {
        private const string TableName = "BuildingUnitAddressSyndicationV2WithCount";

        public void Configure(EntityTypeBuilder<BuildingUnitAddressSyndicationItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.Position, p.BuildingUnitPersistentLocalId, p.AddressPersistentLocalId })
                .IsClustered(false);

            b.Property(e => e.Count).HasDefaultValue(1);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.BuildingUnitPersistentLocalId);
            b.HasIndex(x => x.AddressPersistentLocalId);
        }
    }
}
