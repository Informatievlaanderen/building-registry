namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingExtractItemV2Esri
    {
        public int PersistentLocalId { get; set; }
        public byte[] DbaseRecord { get; set; }
        public byte[]? ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
    }

    public class BuildingExtractItemV2EsriConfiguration : IEntityTypeConfiguration<BuildingExtractItemV2Esri>
    {
        private const string TableName = "BuildingV2Esri";

        public void Configure(EntityTypeBuilder<BuildingExtractItemV2Esri> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            builder.Property(p => p.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.ShapeRecordContent);
            builder.Property(p => p.ShapeRecordContentLength);
            builder.Property(p => p.MaximumX);
            builder.Property(p => p.MinimumX);
            builder.Property(p => p.MinimumY);
            builder.Property(p => p.MaximumY);

            builder.HasIndex(p => p.ShapeRecordContentLength);
            builder.HasIndex(p => p.MaximumX);
            builder.HasIndex(p => p.MinimumX);
            builder.HasIndex(p => p.MaximumY);
            builder.HasIndex(p => p.MinimumY);
        }
    }
}
