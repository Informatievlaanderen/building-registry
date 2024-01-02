namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
{
    using System;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddressLegacyVersion
    {
        public long Position { get; set; }

        public int BuildingUnitPersistentLocalId { get; set; }
        public Guid AddressId { get; set; }

        public BuildingUnitAddressLegacyVersion()
        { }

        public BuildingUnitAddressLegacyVersion CloneAndApplyEventInfo(
            long newPosition)
        {
            var newItem = new BuildingUnitAddressLegacyVersion
            {
                Position = newPosition,
                BuildingUnitPersistentLocalId = BuildingUnitPersistentLocalId,
                AddressId = AddressId
            };

            return newItem;
        }
    }

    public sealed class BuildingUnitAddressLegacyVersionConfiguration : IEntityTypeConfiguration<BuildingUnitAddressLegacyVersion>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitAddressLegacyVersion> builder)
        {
            const string tableName = "building_unit_address_legacy_versions";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new
                {
                    x.Position,
                    x.BuildingUnitPersistentLocalId,
                    x.AddressId
                });

            builder.Property(x => x.Position).ValueGeneratedNever();

            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.BuildingUnitPersistentLocalId).HasColumnName("building_unit_persistent_local_id");
            builder.Property(x => x.AddressId).HasColumnName("address_id");

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.BuildingUnitPersistentLocalId);
            builder.HasIndex(x => x.AddressId);
        }
    }
}
