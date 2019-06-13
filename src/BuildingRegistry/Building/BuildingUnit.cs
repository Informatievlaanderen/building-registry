namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Events;
    using Events.Crab;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using ValueObjects;

    public partial class BuildingUnit : Entity
    {
        private static readonly List<CrabAddressPositionOrigin> CrabAppointedAddressPositionOrigins = new List<CrabAddressPositionOrigin>
        {
            CrabAddressPositionOrigin.ManualIndicationFromMailbox,
            CrabAddressPositionOrigin.ManualIndicationFromBuilding,
            CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding,
            CrabAddressPositionOrigin.ManualIndicationFromBerth,
            CrabAddressPositionOrigin.ManualIndicationFromLot,
            CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection,
            CrabAddressPositionOrigin.ManualIndicationFromParcel,
            CrabAddressPositionOrigin.ManualIndicationFromStand,
            CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad,
        };

        private static readonly List<CrabAddressPositionOrigin> CrabApplicationAddressPositionOrigins = new List<CrabAddressPositionOrigin>
        {
            CrabAddressPositionOrigin.ManualIndicationFromMailbox,
            CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection,
            CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad,
        };

        public AddressId GetCurrentAddressIdsFor(CrabTimestamp timestamp)
        {
            var addressId =
                    _readdressedEvents
                        .Where(x => x.BeginDate.ToDateTimeUnspecified() < ((Instant)timestamp).ToDateTimeOffset())
                        .OrderBy(x => x.BeginDate)
                        .LastOrDefault()
                        ?.NewAddressId;

            if (addressId == null && _readdressedEvents.Any())
                return new AddressId(_readdressedEvents.OrderBy(x => x.BeginDate).First().OldAddressId);
            else
                return AddressIds.SingleOrDefault();
        }

        internal void ApplyRemove()
        {
            Apply(new BuildingUnitWasRemoved(_buildingId, BuildingUnitId));
        }

        internal void ApplyRetired(bool isCorrection)
        {
            if ((Status == BuildingUnitStatus.Retired || Status == BuildingUnitStatus.NotRealized) && !IsRetiredByBuilding)
                return;

            if (Status == BuildingUnitStatus.Realized)
            {
                if (!isCorrection)
                    ApplyStatusChange(BuildingUnitStatus.Retired);
                else
                    ApplyStatusCorrectionChange(BuildingUnitStatus.Retired);
            }
            else
            {
                if (!isCorrection)
                    ApplyStatusChange(BuildingUnitStatus.NotRealized);
                else
                    ApplyStatusCorrectionChange(BuildingUnitStatus.NotRealized);
            }

            if (AddressIds.Any())
                Apply(new BuildingUnitAddressWasDetached(_buildingId, AddressIds, BuildingUnitId));
        }

        public void ApplyRetiredFromParent(BuildingUnitId parentBuildingUnitId)
        {
            if (Status == BuildingUnitStatus.Retired || Status == BuildingUnitStatus.NotRealized && !IsRetiredByBuilding)
                return;

            if (Status == BuildingUnitStatus.Realized)
            {
                Apply(new BuildingUnitWasRetiredByParent(_buildingId, BuildingUnitId, parentBuildingUnitId));
            }
            else
            {
                Apply(new BuildingUnitWasNotRealizedByParent(_buildingId, BuildingUnitId, parentBuildingUnitId));
            }

            if (AddressIds.Any())
                Apply(new BuildingUnitAddressWasDetached(_buildingId, AddressIds, BuildingUnitId));
        }

        internal void ApplyStatusChange(AddressHouseNumberStatusWasImportedFromCrab legacyEvent)
        {
            ApplyStatusChangesFor(_houseNumberStatusChronicle, legacyEvent);
        }

        internal void ApplyStatusChange(AddressSubadressStatusWasImportedFromCrab legacyEvent)
        {
            ApplyStatusChangesFor(_subaddressStatusChronicle, legacyEvent);
        }

        private void ApplyStatusChangesFor<T, TKey>(Chronicle<T, TKey> chronicle,
           T legacyEvent) where T : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<TKey>
        {
            var crabStatusEvent = chronicle.MostCurrent(legacyEvent);
            var newStatus = Map<T, TKey>(crabStatusEvent);

            if (Status != newStatus && (!HasRetiredState || newStatus == BuildingUnitStatus.NotRealized || newStatus == BuildingUnitStatus.Retired))
            {
                if (crabStatusEvent != null && crabStatusEvent.Modification == CrabModification.Correction)
                    ApplyStatusCorrectionChange(newStatus);
                else
                    ApplyStatusChange(newStatus);
            }

            CheckCompleteness();
        }

        private void ApplyStatusChange(BuildingUnitStatus? status)
        {
            if (status == BuildingUnitStatus.Realized)
                Apply(new BuildingUnitWasRealized(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.Retired)
                Apply(new BuildingUnitWasRetired(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.NotRealized)
                Apply(new BuildingUnitWasNotRealized(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.Planned)
                Apply(new BuildingUnitWasPlanned(_buildingId, BuildingUnitId));

            else if (status == null)
                Apply(new BuildingUnitStatusWasRemoved(_buildingId, BuildingUnitId));
        }

        private void ApplyStatusCorrectionChange(BuildingUnitStatus? status)
        {
            if (status == BuildingUnitStatus.Realized)
                Apply(new BuildingUnitWasCorrectedToRealized(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.Retired)
                Apply(new BuildingUnitWasCorrectedToRetired(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.NotRealized)
                Apply(new BuildingUnitWasCorrectedToNotRealized(_buildingId, BuildingUnitId));

            else if (status == BuildingUnitStatus.Planned)
                Apply(new BuildingUnitWasCorrectedToPlanned(_buildingId, BuildingUnitId));
        }

        private BuildingUnitStatus? Map<T, TKey>(T crabStatusEvent)
            where T : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<TKey>
        {
            var newStatus = crabStatusEvent == null
                ? null
                : Map(crabStatusEvent.AddressStatus, crabStatusEvent.Modification);

            return newStatus;
        }

        private BuildingUnitStatus? Map(CrabAddressStatus crabAddressStatus, CrabModification? modification)
        {
            if (modification == CrabModification.Delete)
                return null;

            switch (crabAddressStatus)
            {
                case CrabAddressStatus.Proposed:
                case CrabAddressStatus.Reserved:
                    return !HasRetiredState ? BuildingUnitStatus.Planned : BuildingUnitStatus.NotRealized;
                case CrabAddressStatus.InUse:
                case CrabAddressStatus.OutOfUse:
                case CrabAddressStatus.Unofficial:
                    return !HasRetiredState ? BuildingUnitStatus.Realized : BuildingUnitStatus.Retired;
                default:
                    throw new NotImplementedException();
            }
        }

        internal void ApplyPositionChange(AddressSubaddressPositionWasImportedFromCrab legacyEvent, bool isCorrection)
        {
            var crabPositionEvent = GetLastMostQualitativeCrabPosition<AddressSubaddressPositionWasImportedFromCrab, int>(_subaddressPositionsFromCrab, legacyEvent);

            ApplyPositionChange(isCorrection, crabPositionEvent);
        }

        internal void ApplyPositionChange(AddressHouseNumberPositionWasImportedFromCrab legacyEvent, bool isCorrection)
        {
            var crabPositionEvent = GetLastMostQualitativeCrabPosition<AddressHouseNumberPositionWasImportedFromCrab, int>(_houseNumberPositionsFromCrab, legacyEvent);

            ApplyPositionChange(isCorrection, crabPositionEvent);
        }

        internal void CheckAndCorrectPositionIfNeeded(bool isCorrection, bool shouldIncreaseVersion = true)
        {
            if (_subaddressPositionsFromCrab.Any())
            {
                var crabEvent = GetLastMostQualitativeCrabPosition<AddressSubaddressPositionWasImportedFromCrab, int>(
                    _subaddressPositionsFromCrab, null);

                ApplyPositionChange(isCorrection, crabEvent);
            }
            else
            {
                var crabEvent = GetLastMostQualitativeCrabPosition<AddressHouseNumberPositionWasImportedFromCrab, int>(_houseNumberPositionsFromCrab, null);

                ApplyPositionChange(isCorrection, crabEvent);
            }

            CheckCompleteness();
        }

        private void ApplyPositionChange(bool isCorrection, IHasCrabPosition crabPositionEvent)
        {
            if (BuildingGeometry == null)
                return;

            var position = crabPositionEvent == null ? null : ExtendedWkbGeometry.CreateEWkb(crabPositionEvent.AddressPosition.ToByteArray());
            var method = Map(crabPositionEvent?.AddressPositionOrigin);

            if (position == null || !BuildingGeometry.Contains(position))
            {
                position = BuildingGeometry.Center;
                method = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            }

            var newPosition = new BuildingUnitPosition(position, method);
            if (newPosition == BuildingUnitPosition)
                return;

            ApplyPosition(method, position, isCorrection);
        }

        private static BuildingUnitPositionGeometryMethod Map(CrabAddressPositionOrigin? origin)
        {
            if (origin == null)
                return BuildingUnitPositionGeometryMethod.DerivedFromObject;

            return CrabAppointedAddressPositionOrigins.Contains(origin.Value)
                ? BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                : BuildingUnitPositionGeometryMethod.DerivedFromObject;
        }

        private void ApplyPosition(BuildingUnitPositionGeometryMethod method, ExtendedWkbGeometry position, bool isCorrection)
        {
            if (method == BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                if (!isCorrection)
                    Apply(new BuildingUnitPositionWasDerivedFromObject(_buildingId, BuildingUnitId, position));
                else
                    Apply(new BuildingUnitPositionWasCorrectedToDerivedFromObject(_buildingId, BuildingUnitId, position));
            }
            else
            {
                if (!isCorrection)
                    Apply(new BuildingUnitPositionWasAppointedByAdministrator(_buildingId, BuildingUnitId, position));
                else
                    Apply(new BuildingUnitPositionWasCorrectedToAppointedByAdministrator(_buildingId, BuildingUnitId, position));
            }
        }

        private T GetLastMostQualitativeCrabPosition<T, TKey>(IEnumerable<T> @events, T latestEvent)
            where T : ICrabEvent, IHasCrabPosition, IHasCrabKey<TKey>
        {
            var allPositionEvents = @events.Concat(latestEvent != null ? new[] { latestEvent } : new T[0]);

            var lastEventsPerPositionId = allPositionEvents
                .GroupBy(e => e.Key)
                .Select(group =>
                {
                    var deleteEvent = group.FirstOrDefault(e => e.Modification == CrabModification.Delete);
                    if (deleteEvent != null)
                        return deleteEvent;

                    return group.Last();
                });

            var mostQualitativePosition = lastEventsPerPositionId
                .Where(e => e.Modification != CrabModification.Delete && (!e.EndDateTime.HasValue || HasRetiredState) && !string.IsNullOrEmpty(e.AddressPosition))
                .Where(e => !CrabApplicationAddressPositionOrigins.Contains(e.AddressPositionOrigin))
                .OrderBy(e => e.AddressPositionOrigin, new CrabAddressPositionComparer())
                .LastOrDefault();

            return mostQualitativePosition;
        }

        internal void CheckCompleteness()
        {
            if (BuildingUnitPosition != null && Status != null && !IsComplete)
                Apply(new BuildingUnitBecameComplete(_buildingId, BuildingUnitId));
            if ((BuildingUnitPosition == null || Status == null) && IsComplete)
                Apply(new BuildingUnitBecameIncomplete(_buildingId, BuildingUnitId));
        }

        internal void ApplyDetachAddress(AddressId addressId)
        {
            Apply(new BuildingUnitAddressWasDetached(_buildingId, addressId, BuildingUnitId));
        }

        public void ApplyAttachAddress(AddressId addressId)
        {
            Apply(new BuildingUnitAddressWasAttached(_buildingId, addressId, BuildingUnitId));
        }

        public void ApplyOsloId(OsloId osloId, OsloAssignmentDate assignmentDate)
        {
            Apply(new BuildingUnitOsloIdWasAssigned(_buildingId, BuildingUnitId, osloId, assignmentDate));
        }
    }
}
