namespace BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DuplicatedPersistentLocalId
    {
        public int DuplicatePersistentLocalId { get; set; }
        public Guid BuildingId { get; set; }
        public int OriginalPersistentLocalId { get; set; }
    }

    public class DuplicatedPersistentLocalIdConfiguration : IEntityTypeConfiguration<DuplicatedPersistentLocalId>
    {
        private const string TableName = "DuplicatedPersistentLocalIds";

        public void Configure(EntityTypeBuilder<DuplicatedPersistentLocalId> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.DuplicatePersistentLocalId)
                .IsClustered(false);

            b.Property(x => x.DuplicatePersistentLocalId).ValueGeneratedNever();

            b.Property(x => x.OriginalPersistentLocalId);
            b.Property(x => x.BuildingId);
        }
    }
}
