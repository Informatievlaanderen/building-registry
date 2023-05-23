namespace BuildingRegistry.Grb.Abstractions
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class Job
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset LastChanged { get; private set; }
        public JobStatus Status { get; private set; }
        public Guid? TicketId { get; set; }

        public string UploadBlobName => $"upload_{Id:D}";
        public string ReceivedBlobName => $"received/{Id:D}";

        public static string JobResultsBlobName(Guid id) => $"jobresults/{id:D}";

        private Job() { }

        public Job(DateTimeOffset created, JobStatus status, Guid? ticketId = null)
        {
            Created = created;
            LastChanged = created;
            Status = status;
            TicketId = ticketId;
        }

        public void UpdateStatus(JobStatus status)
        {
            Status = status;
            LastChanged = DateTimeOffset.Now;
        }

        public bool IsExpired(TimeSpan expiration) => Created.Add(expiration) < DateTimeOffset.Now;
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
