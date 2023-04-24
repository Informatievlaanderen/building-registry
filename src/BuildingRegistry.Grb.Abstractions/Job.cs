namespace BuildingRegistry.Grb.Abstractions
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class Job
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastChanged { get; set; }
        public JobStatus Status { get; set; }
        public string TicketUrl { get; set; }

        private Job() { }

        public Job(Guid id, DateTimeOffset created, JobStatus status, string ticketUrl)
        {
            Id = id;
            Created = created;
            LastChanged = created;
            Status = status;
            TicketUrl = ticketUrl;
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
            builder.Property(x => x.TicketUrl);

            builder.HasIndex(x => x.Status);
        }
    }
}
