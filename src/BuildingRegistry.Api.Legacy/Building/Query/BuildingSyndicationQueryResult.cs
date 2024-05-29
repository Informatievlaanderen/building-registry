namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount;
    using NodaTime;

    public class BuildingSyndicationQueryResult
    {
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public string BuildingId { get; }
        public long Position { get; }
        public int? PersistentLocalId { get; }
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
            string buildingId,
            long position,
            int? persistentLocalId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason)
        {
            ContainsEvent = false;
            ContainsObject = false;

            BuildingId = buildingId;
            Position = position;
            PersistentLocalId = persistentLocalId;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            Organisation = organisation;
            Reason = reason;
        }

        public BuildingSyndicationQueryResult(
            string buildingId,
            long position,
            int? persistentLocalId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            string eventDataAsXml)

            : this(buildingId,
                position,
                persistentLocalId,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason)
        {
            ContainsEvent = true;
            EventDataAsXml = eventDataAsXml;
        }

        public BuildingSyndicationQueryResult(
            string buildingId,
            long position,
            int? persistentLocalId,
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
                persistentLocalId,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason)
        {
            ContainsObject = true;

            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry;
            IsComplete = isComplete;
            Organisation = organisation;
            Reason = reason;
            BuildingUnits = buildingUnits.Select(x => new BuildingUnitSyndicationQueryResult(
                x.BuildingUnitId.ToString("D"),
                x.PersistentLocalId,
                x.Function,
                x.Status,
                x.PositionMethod,
                x.PointPosition,
                x.IsComplete,
                x.Addresses,
                x.Readdresses,
                lastChangedOn,
                x.Version,
                false));
        }
        public BuildingSyndicationQueryResult(
            string buildingId,
            long position,
            int? persistentLocalId,
            BuildingStatus? status,
            BuildingGeometryMethod? geometryMethod,
            byte[] geometry,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            IEnumerable<BuildingUnitSyndicationItemV2> buildingUnits)
            : this(buildingId,
                position,
                persistentLocalId,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason)
        {
            ContainsObject = true;

            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry;
            IsComplete = isComplete;
            Organisation = organisation;
            Reason = reason;
            BuildingUnits = buildingUnits.Select(x => new BuildingUnitSyndicationQueryResult(
                x.PersistentLocalId,
                x.Function,
                x.Status,
                x.PositionMethod,
                x.PointPosition,
                x.Addresses,
                lastChangedOn,
                x.Version,
                x.HasDeviation));
        }

        public BuildingSyndicationQueryResult(
            string buildingId,
            long position,
            int? persistentLocalId,
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
                persistentLocalId,
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

        public BuildingSyndicationQueryResult(
            string buildingId,
            long position,
            int? persistentLocalId,
            BuildingStatus? status,
            BuildingGeometryMethod? geometryMethod,
            byte[] geometry,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            IEnumerable<BuildingUnitSyndicationItemV2> buildingUnits,
            string eventDataAsXml)
            : this(buildingId,
                position,
                persistentLocalId,
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
        public string BuildingUnitId { get; }
        public int? PersistentLocalId { get; }
        public BuildingUnitFunction? Function { get; }
        public BuildingUnitStatus? Status { get; }
        public BuildingUnitPositionGeometryMethod? GeometryMethod { get; }
        public byte[] Geometry { get; }
        public bool IsComplete { get; }
        public Instant Version { get; }
        public IEnumerable<string> AddressIds { get; }
        public bool HasDeviation { get; }

        public BuildingUnitSyndicationQueryResult(string buildingUnitId,
            int? persistentLocalId,
            BuildingUnitFunction? function,
            BuildingUnitStatus? status,
            BuildingUnitPositionGeometryMethod? geometryMethod,
            byte[] geometry,
            bool isComplete,
            IEnumerable<BuildingUnitAddressSyndicationItem> addresses,
            IEnumerable<BuildingUnitReaddressSyndicationItem> readdresses,
            Instant lastChangedOn,
            Instant version,
            bool hasDeviation)
        {
            BuildingUnitId = buildingUnitId;
            PersistentLocalId = persistentLocalId;
            Function = function;
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry;
            IsComplete = isComplete;
            Version = version;
            HasDeviation = hasDeviation;

            var datetimeLastChangedOn = lastChangedOn.ToBelgianDateTimeOffset();
            var relevantReaddresses = readdresses.Where(x =>
                x.ReaddressBeginDate <= LocalDate.FromDateTime(datetimeLastChangedOn.DateTime)).ToList();

            AddressIds = addresses
                .Where(x => x.AddressId.HasValue)
                .Select(address => relevantReaddresses.Any(x => x.OldAddressId == address.AddressId)
                    ? relevantReaddresses.First(x => x.OldAddressId == address.AddressId).NewAddressId
                    : address.AddressId.Value)
                .Distinct()
                .Select(x => x.ToString("D"))
                .ToList();
        }

        public BuildingUnitSyndicationQueryResult(int persistentLocalId,
            BuildingRegistry.Building.BuildingUnitFunction function,
            BuildingRegistry.Building.BuildingUnitStatus status,
            BuildingRegistry.Building.BuildingUnitPositionGeometryMethod geometryMethod,
            byte[] geometry,
            IEnumerable<BuildingUnitAddressSyndicationItemV2> addresses,
            Instant lastChangedOn,
            Instant version,
            bool hasDeviation)
        {
            BuildingUnitId = persistentLocalId.ToString();
            PersistentLocalId = persistentLocalId;
            Function = BuildingUnitFunction.Parse(function.Function);
            Status = BuildingUnitStatus.Parse(status.Status);
            GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(geometryMethod.GeometryMethod);
            Geometry = geometry;
            IsComplete = true;
            Version = version;
            HasDeviation = hasDeviation;

            AddressIds = addresses.Select(address => address.AddressPersistentLocalId.ToString());
        }
    }
}
