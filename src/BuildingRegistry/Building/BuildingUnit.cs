namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Commands;
    using Datastructures;
    using Events;
    using Exceptions;

    public partial class BuildingUnit : Entity
    {
        private static BuildingUnitStatus[] StatusesWhichCanBeRealized => new[] { BuildingUnitStatus.Planned };
        private static BuildingUnitStatus[] StatusesWhichCanBeNotRealized => new[] { BuildingUnitStatus.Planned };

        public static BuildingUnit Migrate(
            Action<object> applier,
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitFunction function,
            BuildingUnitStatus status,
            List<AddressPersistentLocalId> addressPersistentLocalIds,
            BuildingUnitPosition buildingUnitPosition,
            bool isRemoved)
        {
            var unit = new BuildingUnit(applier)
            {
                _buildingPersistentLocalId = buildingPersistentLocalId,
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                Function = function,
                Status = status,
                _addressPersistentLocalIds = addressPersistentLocalIds,
                BuildingUnitPosition = buildingUnitPosition,
                IsRemoved = isRemoved,
            };

            return unit;
        }

        public void Realize()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(StatusesWhichCanBeRealized);

            Apply(new BuildingUnitWasRealizedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void RealizeBecauseBuildingWasRealized()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            if (!StatusesWhichCanBeRealized.Contains(Status))
            {
                return;
            }

            Apply(new BuildingUnitWasRealizedBecauseBuildingWasRealized(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void NotRealize()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.NotRealized)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(StatusesWhichCanBeNotRealized);

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            Apply(new BuildingUnitWasNotRealizedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void NotRealizeBecauseBuildingWasNotRealized()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == BuildingUnitStatus.NotRealized)
            {
                return;
            }

            if (!StatusesWhichCanBeNotRealized.Contains(Status))
            {
                return;
            }

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            Apply(new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void Retire()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Retired)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Realized);

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            Apply(new BuildingUnitWasRetiredV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void Remove()
        {
            GuardCommonUnit();

            if (IsRemoved)
            {
                return;
            }

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            Apply(new BuildingUnitWasRemovedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void RemoveBecauseBuildingWasRemoved()
        {
            if (IsRemoved)
            {
                return;
            }

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            Apply(new BuildingUnitWasRemovedBecauseBuildingWasRemoved(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void Regularize()
        {
            GuardRemoved();
            GuardCommonUnit();

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            if (!HasDeviation)
            {
                return;
            }

            Apply(new BuildingUnitWasRegularized(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void Deregulate()
        {
            GuardRemoved();
            GuardCommonUnit();

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            if (HasDeviation)
            {
                return;
            }

            Apply(new BuildingUnitWasDeregulated(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId));
        }

        public void CorrectRealization()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Planned)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Realized);

            Apply(new BuildingUnitWasCorrectedFromRealizedToPlanned(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectRealizationBecauseBuildingWasCorrected()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == BuildingUnitStatus.Realized)
            {
                Apply(new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
            }
        }

        public void CorrectNotRealization(BuildingGeometry buildingGeometry)
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Planned)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.NotRealized);

            var correctedBuildingUnitPosition = CorrectedBuildingUnitPosition(buildingGeometry);

            if (correctedBuildingUnitPosition is not null)
            {
                Apply(new BuildingUnitPositionWasCorrected(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    correctedBuildingUnitPosition));
            }

            Apply(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId));
        }

        public void CorrectRetirement(BuildingGeometry buildingGeometry)
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Retired);

            var correctedBuildingUnitPosition = CorrectedBuildingUnitPosition(buildingGeometry);

            if (correctedBuildingUnitPosition is not null)
            {
                Apply(new BuildingUnitPositionWasCorrected(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    correctedBuildingUnitPosition));
            }

            Apply(new BuildingUnitWasCorrectedFromRetiredToRealized(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId));
        }

        public void CorrectPosition(BuildingUnitPositionGeometryMethod positionGeometryMethod, ExtendedWkbGeometry finalPosition)
        {
            GuardRemoved();
            GuardCommonUnit();
            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            Apply(new BuildingUnitPositionWasCorrected(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition));
        }

        public void CorrectRegularization()
        {
            GuardRemoved();
            GuardCommonUnit();

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            if (HasDeviation)
            {
                return;
            }

            Apply(new BuildingUnitRegularizationWasCorrected(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectDeregulation()
        {
            GuardRemoved();
            GuardCommonUnit();

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            if (!HasDeviation)
            {
                return;
            }

            Apply(new BuildingUnitDeregulationWasCorrected(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId));
        }

        public void AttachAddress(AddressPersistentLocalId addressPersistentLocalId, IAddresses addresses)
        {
            GuardRemoved();

            GuardValidBuildingUnitStatuses(BuildingUnitStatus.Planned, BuildingUnitStatus.Realized);

            if (AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            var address = addresses.GetOptional(addressPersistentLocalId);

            GuardRemovedAddress(address);

            var validAddressStatuses = new[] { AddressStatus.Current, AddressStatus.Proposed };

            if (!validAddressStatuses.Contains(address!.Value.Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            Apply(new BuildingUnitAddressWasAttachedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId, addressPersistentLocalId));
        }

        public void DetachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardRemoved();

            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            Apply(new BuildingUnitAddressWasDetachedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRejected(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            Apply(new BuildingUnitAddressWasDetachedBecauseAddressWasRejected(_buildingPersistentLocalId, BuildingUnitPersistentLocalId, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRetired(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            Apply(new BuildingUnitAddressWasDetachedBecauseAddressWasRetired(_buildingPersistentLocalId, BuildingUnitPersistentLocalId, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRemoved(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            Apply(new BuildingUnitAddressWasDetachedBecauseAddressWasRemoved(_buildingPersistentLocalId, BuildingUnitPersistentLocalId, addressPersistentLocalId));
        }

        public BuildingUnitAddressesWereReaddressed? BuildBuildingUnitAddressesWereReaddressed(IReadOnlyList<ReaddressData> readdresses)
        {
            var addressPersistentLocalIdsToAttach = readdresses
                .Select(x => x.DestinationAddressPersistentLocalId)
                .Except(readdresses.Select(x => x.SourceAddressPersistentLocalId))
                .Except(AddressPersistentLocalIds)
                .ToList();

            var addressPersistentLocalIdsToDetach = readdresses
                .Select(x => x.SourceAddressPersistentLocalId)
                .Except(readdresses.Select(x => x.DestinationAddressPersistentLocalId))
                .Where(AddressPersistentLocalIds.Contains)
                .ToList();

            if (!addressPersistentLocalIdsToAttach.Any() && !addressPersistentLocalIdsToDetach.Any())
            {
                return null;
            }

            return new BuildingUnitAddressesWereReaddressed(
                BuildingUnitPersistentLocalId,
                addressPersistentLocalIdsToAttach,
                addressPersistentLocalIdsToDetach
            );
        }

        private ExtendedWkbGeometry? CorrectedBuildingUnitPosition(BuildingGeometry buildingGeometry)
        {
            var correctedBuildingUnitPosition =
                !buildingGeometry.Contains(BuildingUnitPosition.Geometry)
                || BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject
                && BuildingUnitPosition.Geometry != buildingGeometry.Center
                    ? buildingGeometry.Center
                    : null;
            return correctedBuildingUnitPosition;
        }

        private void GuardRemoved()
        {
            if (IsRemoved)
            {
                throw new BuildingUnitIsRemovedException(BuildingUnitPersistentLocalId);
            }
        }

        private void GuardValidBuildingUnitStatuses(params BuildingUnitStatus[] validStatuses)
        {
            if (!validStatuses.Contains(Status))
            {
                throw new BuildingUnitHasInvalidStatusException();
            }
        }

        internal void GuardCommonUnit()
        {
            if (Function == BuildingUnitFunction.Common)
            {
                throw new BuildingUnitHasInvalidFunctionException();
            }
        }

        private void GuardRemovedAddress(AddressData? address)
        {
            if (address is null)
            {
                throw new AddressNotFoundException();
            }

            if (address.Value.IsRemoved)
            {
                throw new AddressIsRemovedException();
            }
        }

        public void RestoreSnapshot(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingSnapshot.BuildingUnitData buildingUnitData)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitData.BuildingUnitPersistentLocalId);
            Status = BuildingUnitStatus.Parse(buildingUnitData.Status);
            Function = BuildingUnitFunction.Parse(buildingUnitData.Function);
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(buildingUnitData.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(buildingUnitData.GeometryMethod));

            _addressPersistentLocalIds =
                buildingUnitData.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)).ToList();

            IsRemoved = buildingUnitData.IsRemoved;
            HasDeviation = buildingUnitData.HasDeviation;

            _lastSnapshotEventHash = buildingUnitData.LastEventHash;
            _lastSnapshotProvenance = buildingUnitData.LastProvenanceData;
        }
    }
}
