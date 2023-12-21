namespace BuildingRegistry.Projections.Integration
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddressVersion
    {
        public long Position { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        public BuildingUnitAddressVersion()
        { }

        public BuildingUnitAddressVersion CloneAndApplyEventInfo(
            long newPosition)
        {
            var newItem = new BuildingUnitAddressVersion
            {
                Position = newPosition,
                BuildingUnitPersistentLocalId = BuildingUnitPersistentLocalId,
                AddressPersistentLocalId = AddressPersistentLocalId
            };

            return newItem;
        }
    }

    public sealed class BuildingUnitAddressVersionConfiguration : IEntityTypeConfiguration<BuildingUnitAddressVersion>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitAddressVersion> builder)
        {
            const string tableName = "building_unit_address_versions";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new
                {
                    x.Position,
                    x.BuildingUnitPersistentLocalId,
                    x.AddressPersistentLocalId
                });

            builder.Property(x => x.Position).ValueGeneratedNever();

            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.BuildingUnitPersistentLocalId).HasColumnName("building_unit_persistent_local_id");
            builder.Property(x => x.AddressPersistentLocalId).HasColumnName("address_persistent_local_id");

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.BuildingUnitPersistentLocalId);
            builder.HasIndex(x => x.AddressPersistentLocalId);
        }
    }
}
