namespace BuildingRegistry.Consumer.Address
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OffsetOverride
    {
        public string ConsumerGroupId { get; set; }
        public long Offset { get; set; }
        public bool Configured { get; set; }

        //Needed for EF
        private OffsetOverride()
        { }
    }

    public class OffsetOverrideConfiguration : IEntityTypeConfiguration<OffsetOverride>
    {
        public const string TableName = "OffsetOverrides";

        public void Configure(EntityTypeBuilder<OffsetOverride> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerAddress)
                .HasKey(x => x.ConsumerGroupId);
        }
    }
}
