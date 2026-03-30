namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class BuildingDocument
    {
        public int PersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public BuildingDocumentContent Document { get; set; }

        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }
        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set
            {
                var belgianDateTimeOffset = value.ToBelgianDateTimeOffset();
                LastChangedOnAsDateTimeOffset = belgianDateTimeOffset;
                Document.VersionId = belgianDateTimeOffset;
            }
        }

        private BuildingDocument()
        {
            Document = new BuildingDocumentContent();
            IsRemoved = false;
        }

        public BuildingDocument(
            int persistentLocalId,
            GebouwStatus status,
            GeometrieMethode geometryMethod,
            Instant createdTimestamp)
        {
            PersistentLocalId = persistentLocalId;
            RecordCreatedAt = createdTimestamp;

            Document = new BuildingDocumentContent
            {
                PersistentLocalId = persistentLocalId,
                Status = status,
                GeometryMethod = geometryMethod,
            };

            LastChangedOn = createdTimestamp;
        }
    }

    public sealed class BuildingDocumentContent
    {
        public int PersistentLocalId { get; set; }
        public GebouwStatus Status { get; set; }
        public GeometrieMethode GeometryMethod { get; set; }
        public string GeometryAsGml { get; set; } = string.Empty;
        public string ExtendedWkbGeometry { get; set; } = string.Empty;

        public DateTimeOffset VersionId { get; set; }
    }

    public sealed class BuildingDocumentConfiguration : IEntityTypeConfiguration<BuildingDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public BuildingDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<BuildingDocument> b)
        {
            b.ToTable("BuildingDocuments", Schema.Feed)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            b.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.LastChangedOnAsDateTimeOffset)
                .HasColumnName("LastChangedOn");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset)
                .HasColumnName("RecordCreatedAt");

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<BuildingDocumentContent>(v, _serializerSettings)!);

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);
        }
    }
}
