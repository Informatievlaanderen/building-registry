namespace BuildingRegistry.Grb.Abstractions
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore.ValueGeneration;

    public sealed class Job
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastChanged { get; set; }
        public JobStatus Status { get; set; }
        public Guid? TicketId { get; set; }

        public string BlobName => $"upload_{Id:D}";

        private Job() { }

        public Job(DateTimeOffset created, JobStatus status, Guid? ticketId = null)
        {
            Created = created;
            LastChanged = created;
            Status = status;
            TicketId = ticketId;
        }
    }

    public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder
                .ToTable("Jobs", BuildingGrbContext.Schema)
                .HasKey(x => x.Id);

            builder
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Created);
            builder.Property(x => x.LastChanged);
            builder.Property(x => x.Status);
            builder.Property(x => x.TicketId);

            builder.HasIndex(x => x.Status);
        }
    }
}
