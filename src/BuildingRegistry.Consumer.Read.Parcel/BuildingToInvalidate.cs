namespace BuildingRegistry.Consumer.Read.Parcel
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingToInvalidate
    {
        public int Id { get; set; }
        public int BuildingPersistentLocalId { get; set; }
    }

    public class BuildingToInvalidateConfiguration : IEntityTypeConfiguration<BuildingToInvalidate>
    {
        public const string TableName = "BuildingsToInvalidate";

        public void Configure(EntityTypeBuilder<BuildingToInvalidate> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadParcel)
                .HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.Id).UseIdentityColumn(1).ValueGeneratedOnAdd();

            builder.Property(x => x.BuildingPersistentLocalId);
        }
    }
}
