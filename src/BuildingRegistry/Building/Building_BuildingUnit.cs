namespace BuildingRegistry.Building
{
    using System.Linq;
    using Events;
    using Exceptions;

    public partial class Building
    {
        public void PlanBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position,
            BuildingUnitFunction function,
            bool hasDeviation)
        {
            GuardRemovedBuilding();
            GuardActiveBuilding();

            if (_buildingUnits.HasPersistentLocalId(new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)))
            {
                throw new BuildingUnitPersistentLocalIdAlreadyExistsException();
            }

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            ApplyChange(new BuildingUnitWasPlannedV2(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition,
                function,
                hasDeviation));

            EnsureCommonBuildingUnit();
        }

        public void RealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(
                BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Realize();
        }

        public void CorrectRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectRealizeBuildingUnit();
        }

        public void CorrectRetiredBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(
                BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectRetiredBuildingUnit(BuildingGeometry);

            EnsureCommonBuildingUnit();
        }

        public void NotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .NotRealize();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void CorrectNotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectNotRealize(BuildingGeometry);

            EnsureCommonBuildingUnit();
        }

        public void CorrectPositionBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(BuildingStatus.NotRealized, BuildingStatus.Retired);

            var buildingUnit = _buildingUnits.GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId);

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            buildingUnit.CorrectPosition(positionGeometryMethod, finalPosition);
        }

        public void RetireBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Retire();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RemoveBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            _buildingUnits
                .GetByPersistentLocalId(buildingUnitPersistentLocalId)
                .Remove();

            if (_buildingUnits.HasCommonBuildingUnit()
                && !_buildingUnits.NonCommonBuildingUnits().Any())
            {
                ApplyChange(new BuildingUnitWasRemovedV2(
                    BuildingPersistentLocalId,
                    _buildingUnits.CommonBuildingUnit().BuildingUnitPersistentLocalId));
            }

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RegularizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Regularize();
        }

        public void DeregulateBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(BuildingStatus.NotRealized, BuildingStatus.Retired);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Deregulate();
        }

        public void AttachAddressToBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .AttachAddress(addressPersistentLocalId, _addresses);
        }

        public void DetachAddressFromBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddress(addressPersistentLocalId, _addresses);
        }

        public void DetachAddressFromBuildingUnitBecauseAddressWasRemoved(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRemoved(addressPersistentLocalId);
        }

        public void DetachAddressFromBuildingUnitBecauseAddressWasRejected(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRejected(addressPersistentLocalId);
        }

        public void DetachAddressFromBuildingUnitBecauseAddressWasRetired(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRetired(addressPersistentLocalId);
        }

        private void GuardBuildingInvalidStatuses(params BuildingStatus[] invalidStatuses)
        {
            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
        }

        private void EnsureCommonBuildingUnit()
        {
            if (_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || !_buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            if (!_buildingUnits.HasCommonBuildingUnit(excludeRemoved: false))
            {
                AddCommonBuildingUnit(_addCommonBuildingUnit);
                return;
            }

            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit(excludeRemoved: false);
            if (commonBuildingUnit.IsRemoved)
            {
                ApplyChange(new BuildingUnitRemovalWasCorrected(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction
                        ? BuildingUnitStatus.Planned
                        : BuildingUnitStatus.Realized,
                    BuildingUnitFunction.Common,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingGeometry.Center,
                    commonBuildingUnit.HasDeviation,
                    commonBuildingUnit.AddressPersistentLocalIds));
            }
            else
            {
                CorrectCommonBuildingUnitToPlannedOrRealized();
            }
        }

        private void AddCommonBuildingUnit(IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            var commonBuildingUnitPersistentLocalId =
                new BuildingUnitPersistentLocalId(addCommonBuildingUnit.GenerateNextPersistentLocalId());

            var commonBuildingUnitStatus = BuildingStatus == BuildingStatus.Realized
                ? BuildingUnitStatus.Realized
                : BuildingUnitStatus.Planned;

            ApplyChange(new CommonBuildingUnitWasAddedV2(
                BuildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                commonBuildingUnitStatus,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                BuildingGeometry.Center,
                hasDeviation: false));

            addCommonBuildingUnit.AddForBuilding(BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId);
        }

        private void CorrectCommonBuildingUnitToPlannedOrRealized()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if ((BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction)
                && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
                ApplyChange(new BuildingUnitWasRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.Retired)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromRetiredToRealized(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }

        private void CorrectBuildingPosition()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if (commonBuildingUnit.BuildingUnitPosition.Geometry != BuildingGeometry.Center)
            {
                ApplyChange(new BuildingUnitPositionWasCorrected(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingGeometry.Center));
            }
        }

        private void NotRealizeOrRetireCommonBuildingUnit()
        {
            if (!_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || _buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if (commonBuildingUnit.Status == BuildingUnitStatus.Planned)
            {
                ApplyChange(new BuildingUnitWasNotRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (commonBuildingUnit.Status == BuildingUnitStatus.Realized)
            {
                foreach (var addressPersistentLocalId in commonBuildingUnit.AddressPersistentLocalIds.ToList())
                {
                    ApplyChange(new BuildingUnitAddressWasDetachedV2(
                        BuildingPersistentLocalId,
                        commonBuildingUnit.BuildingUnitPersistentLocalId,
                        addressPersistentLocalId));
                }

                ApplyChange(new BuildingUnitWasRetiredV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }
    }
}
