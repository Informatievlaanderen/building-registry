namespace BuildingRegistry.Grb.Abstractions
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class JobResult
    {
        public long Id { get; set; }
        public Guid JobId { get; set; }
        public int GrbIdn { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public bool IsBuildingCreated { get; set; }
    }

    public sealed class JobResultConfiguration : IEntityTypeConfiguration<JobResult>
    {
        public const string TableName = "JobResults";

        public void Configure(EntityTypeBuilder<JobResult> builder)
        {
            builder.ToTable(TableName, BuildingGrbContext.Schema)
                .HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.JobId);
            builder.Property(x => x.GrbIdn);
            builder.Property(x => x.BuildingPersistentLocalId);
            builder.Property(x => x.IsBuildingCreated);
        }
    }
}
