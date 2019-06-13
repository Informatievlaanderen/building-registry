namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NodaTime;
    using Projections.Legacy.BuildingSyndication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public class BuildingSyndicationQueryResult
    {
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public Guid BuildingId { get; }
        public long Position { get; }
        public int? OsloId { get; }
        public string ChangeType { get; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public BuildingStatus? Status { get; }
        public BuildingGeometryMethod? GeometryMethod { get; }
        public byte[] Geometry { get; }
        public bool IsComplete { get; }
        public Organisation? Organisation { get; }
        public string Reason { get; }
        public IEnumerable<BuildingUnitSyndicationQueryResult> BuildingUnits { get; }
        public string EventDataAsXml { get; }

        public BuildingSyndicationQueryResult(
            Guid buildingId,
            long position,
            int? osloId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn)
        {
            ContainsEvent = false;
            ContainsObject = false;

            BuildingId = buildingId;
            Position = position;
            OsloId = osloId;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
        }

        public BuildingSyndicationQueryResult(
            Guid buildingId,
            long position,
            int? osloId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            string eventDataAsXml)

            : this(buildingId,
                position,
                osloId,
                changeType,
                recordCreateAt,
                lastChangedOn)
        {
            ContainsEvent = true;
            EventDataAsXml = eventDataAsXml;
        }

        public BuildingSyndicationQueryResult(
            Guid buildingId,
            long position,
            int? osloId,
            BuildingStatus? status,
            BuildingGeometryMethod? geometryMethod,
            byte[] geometry,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            IEnumerable<BuildingUnitSyndicationItem> buildingUnits)
            : this(buildingId,
                position,
                osloId,
                changeType,
                recordCreateAt,
                lastChangedOn)
        {
            ContainsObject = true;

            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry;
            IsComplete = isComplete;
            Organisation = organisation;
            Reason = reason;
            BuildingUnits = buildingUnits.Select(x => new BuildingUnitSyndicationQueryResult(
                x.BuildingUnitId,
                x.OsloId,
                x.Function,
                x.Status,
                x.PositionMethod,
                x.PositionWkbHex.ToByteArray(),
                x.IsComplete,
                x.Addresses,
                x.Readdresses,
                lastChangedOn,
                x.Version));
        }

        public BuildingSyndicationQueryResult(
            Guid buildingId,
            long position,
            int? osloId,
            BuildingStatus? status,
            BuildingGeometryMethod? geometryMethod,
            byte[] geometry,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            IEnumerable<BuildingUnitSyndicationItem> buildingUnits,
            string eventDataAsXml)
            : this(buildingId,
                position,
                osloId,
                status,
                geometryMethod,
                geometry,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason,
                buildingUnits)
        {
            ContainsEvent = true;
            ContainsObject = true;

            EventDataAsXml = eventDataAsXml;
        }
    }

    public class BuildingUnitSyndicationQueryResult
    {
        public Guid BuildingUnitId { get; }
        public int? OsloId { get; }
        public BuildingUnitFunction? Function { get; }
        public BuildingUnitStatus? Status { get; }
        public BuildingUnitPositionGeometryMethod? GeometryMethod { get; }
        public byte[] Geometry { get; }
        public bool IsComplete { get; }
        public Instant Version { get; }
        public IEnumerable<Guid> AddressIds { get; }

        public BuildingUnitSyndicationQueryResult(
            Guid buildingUnitId,
            int? osloId,
            BuildingUnitFunction? function,
            BuildingUnitStatus? status,
            BuildingUnitPositionGeometryMethod? geometryMethod,
            byte[] geometry,
            bool isComplete,
            IEnumerable<BuildingUnitAddressSyndicationItem> addresses,
            IEnumerable<BuildingUnitReaddressSyndicationItem> readdresses,
            Instant lastChangedOn,
            Instant version)
        {
            BuildingUnitId = buildingUnitId;
            OsloId = osloId;
            Function = function;
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry;
            IsComplete = isComplete;
            Version = version;

            var datetimeLastChangedOn = lastChangedOn.ToBelgianDateTimeOffset();
            var relevantReaddresses = readdresses.Where(x => x.ReaddressBeginDate >= LocalDate.FromDateTime(datetimeLastChangedOn.DateTime)).ToList();

            AddressIds = addresses
                .Where(x => x.AddressId.HasValue)
                .Select(address => relevantReaddresses.Any(x => x.OldAddressId == address.AddressId)
                    ? relevantReaddresses.First(x => x.OldAddressId == address.AddressId).NewAddressId
                    : address.AddressId.Value)
                .ToList();
        }
    }
}
