namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitExtractItemV2
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        
        public byte[] DbaseRecord { get; set; }
        public byte[]? ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
    }

    public class BuildingUnitExtractItemV2Configuration : IEntityTypeConfiguration<BuildingUnitExtractItemV2>
    {
        private const string TableName = "BuildingUnitV2";

        public void Configure(EntityTypeBuilder<BuildingUnitExtractItemV2> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.BuildingUnitPersistentLocalId)
                .IsClustered();

            builder.Property(p => p.BuildingUnitPersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.BuildingPersistentLocalId);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.ShapeRecordContent);
            builder.Property(p => p.ShapeRecordContentLength);
            builder.Property(p => p.MaximumX);
            builder.Property(p => p.MinimumX);
            builder.Property(p => p.MinimumY);
            builder.Property(p => p.MaximumY);

            builder.HasIndex(p => p.BuildingPersistentLocalId);
        }
    }
}
