namespace BuildingRegistry.Projections.Syndication.Parcel
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class BuildingParcelLatestItem
    {
        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }

        public PerceelStatus? Status { get; set; }
        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }

        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class ParcelItemConfiguration : IEntityTypeConfiguration<BuildingParcelLatestItem>
    {
        private const string TableName = "BuildingParcelLatestItems";

        public void Configure(EntityTypeBuilder<BuildingParcelLatestItem> b)
        {
            b.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.ParcelId)
                .ForSqlServerIsClustered();

            b.Property(x => x.CaPaKey);
            b.Property(x => x.Status);

            b.Property(x => x.Version);
            b.Property(x => x.Position);

            b.Property(x => x.IsComplete);
            b.Property(x => x.IsRemoved);

            b.HasIndex(x => x.CaPaKey);
        }
    }
}
