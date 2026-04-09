namespace BuildingRegistry.Projections.Feed.BuildingUnitFeed
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class BuildingUnitDocument
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public BuildingUnitDocumentContent Document { get; set; }

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

        private BuildingUnitDocument()
        {
            Document = new BuildingUnitDocumentContent();
            IsRemoved = false;
        }

        public BuildingUnitDocument(
            int buildingUnitPersistentLocalId,
            int buildingPersistentLocalId,
            GebouweenheidStatus status,
            GebouweenheidFunctie function,
            PositieGeometrieMethode geometryMethod,
            bool hasDeviation,
            Instant createdTimestamp)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
            RecordCreatedAt = createdTimestamp;

            Document = new BuildingUnitDocumentContent
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                Status = status,
                Function = function,
                GeometryMethod = geometryMethod,
                HasDeviation = hasDeviation,
            };

            LastChangedOn = createdTimestamp;
        }
    }

    public sealed class BuildingUnitDocumentContent
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public GebouweenheidStatus Status { get; set; }
        public GebouweenheidFunctie Function { get; set; }
        public PositieGeometrieMethode GeometryMethod { get; set; }
        public string PositionAsGml { get; set; } = string.Empty;
        public string ExtendedWkbGeometry { get; set; } = string.Empty;
        public List<int> AddressPersistentLocalIds { get; set; } = new();
        public bool HasDeviation { get; set; }

        public DateTimeOffset VersionId { get; set; }
    }

    public sealed class BuildingUnitDocumentConfiguration : IEntityTypeConfiguration<BuildingUnitDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public BuildingUnitDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<BuildingUnitDocument> b)
        {
            b.ToTable("BuildingUnitDocuments", Schema.Feed)
                .HasKey(x => x.BuildingUnitPersistentLocalId)
                .IsClustered();

            b.Property(x => x.BuildingUnitPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.BuildingPersistentLocalId);

            b.Property(x => x.LastChangedOnAsDateTimeOffset)
                .HasColumnName("LastChangedOn");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset)
                .HasColumnName("RecordCreatedAt");

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<BuildingUnitDocumentContent>(v, _serializerSettings)!);

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);

            b.HasIndex(x => x.BuildingPersistentLocalId);
        }
    }
}
