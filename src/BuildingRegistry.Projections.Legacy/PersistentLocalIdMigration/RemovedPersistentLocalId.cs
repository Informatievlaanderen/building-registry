namespace BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RemovedPersistentLocalId
    {
        public string PersistentLocalId { get; set; }
        public Guid BuildingId { get; set; }
        public string? Reason { get; set; }
    }

    public class RemovedPersistentLocalIdConfiguration : IEntityTypeConfiguration<RemovedPersistentLocalId>
    {
        private const string TableName = "RemovedPersistentLocalIds";

        public void Configure(EntityTypeBuilder<RemovedPersistentLocalId> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered(false);

            b.Property(x => x.Reason);
            b.Property(x => x.BuildingId);
        }
    }
}
