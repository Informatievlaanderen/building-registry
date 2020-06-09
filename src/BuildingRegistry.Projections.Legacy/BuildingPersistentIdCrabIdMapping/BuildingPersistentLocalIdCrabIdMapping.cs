namespace BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingPersistentLocalIdCrabIdMapping
    {
        public Guid BuildingId { get; set; }
        public int? PersistentLocalId { get; set; }
        public int? CrabTerrainObjectId { get; set; }
        public string? CrabIdentifierTerrainObject { get; set; }
    }

    public class BuildingPersistentLocalIdCrabIdMappingConfiguration : IEntityTypeConfiguration<BuildingPersistentLocalIdCrabIdMapping>
    {
        internal const string TableName = "BuildingPersistentIdCrabIdMappings";

        public void Configure(EntityTypeBuilder<BuildingPersistentLocalIdCrabIdMapping> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingId)
                .IsClustered(false);

            b.Property(p => p.PersistentLocalId);
            b.Property(p => p.CrabTerrainObjectId);
            b.Property(p => p.CrabIdentifierTerrainObject);

            b.HasIndex(p => p.PersistentLocalId).IsClustered();

            b.HasIndex(b => b.CrabTerrainObjectId);
            b.HasIndex(b => b.CrabIdentifierTerrainObject);
        }
    }
}
