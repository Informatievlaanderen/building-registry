namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitExtractItem
    {
        public Guid BuildingUnitId { get; set; }
        public Guid? BuildingId { get; set; }
        public int? PersistentLocalId { get; set; }
        public bool IsComplete { get; set; }
        public bool IsBuildingComplete { get; set; }
        public byte[] DbaseRecord { get; set; }
        public byte[]? ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
    }

    public class BuildingUnitExtractItemConfiguration : IEntityTypeConfiguration<BuildingUnitExtractItem>
    {
        private const string TableName = "BuildingUnit";

        public void Configure(EntityTypeBuilder<BuildingUnitExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.BuildingUnitId)
                .IsClustered(false);

            builder.Property(p => p.PersistentLocalId);
            builder.Property(p => p.BuildingId);
            builder.Property(p => p.IsComplete);
            builder.Property(p => p.IsBuildingComplete);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.ShapeRecordContent);
            builder.Property(p => p.ShapeRecordContentLength);
            builder.Property(p => p.MaximumX);
            builder.Property(p => p.MinimumX);
            builder.Property(p => p.MinimumY);
            builder.Property(p => p.MaximumY);

            builder.HasIndex(p => p.BuildingId);
            builder.HasIndex(p => new { p.IsComplete, p.IsBuildingComplete });
            builder.HasIndex(p => p.PersistentLocalId).IsClustered();
        }
    }
}
