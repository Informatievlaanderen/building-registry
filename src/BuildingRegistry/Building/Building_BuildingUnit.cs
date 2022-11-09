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

            AddOrUpdateStatusCommonBuildingUnit();
        }

        public void RealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(new[]
            {
                BuildingStatus.Planned,
                BuildingStatus.UnderConstruction,
                BuildingStatus.NotRealized,
                BuildingStatus.Retired
            });

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Realize();
        }

        public void CorrectRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.CorrectRealizeBuildingUnit();
        }

        public void CorrectRetiredBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(new[]
            {
                BuildingStatus.Planned,
                BuildingStatus.UnderConstruction,
                BuildingStatus.NotRealized,
                BuildingStatus.Retired
            });

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.CorrectRetiredBuildingUnit(BuildingGeometry);

            AddOrUpdateStatusCommonBuildingUnit();
        }

        public void NotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.NotRealize();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void CorrectNotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(new[]
            {
                BuildingStatus.NotRealized,
                BuildingStatus.Retired
            });

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.CorrectNotRealize(BuildingGeometry);

            AddOrUpdateStatusCommonBuildingUnit();
        }

        public void CorrectPositionBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position)
        {
            GuardRemovedBuilding();
            GuardBuildingInvalidStatuses(new[]
            {
                BuildingStatus.NotRealized,
                BuildingStatus.Retired
            });

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

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
            GuardBuildingInvalidStatuses(new[]
            {
                BuildingStatus.NotRealized,
                BuildingStatus.Retired
            });

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Retire();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RemoveBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Remove();

            if (_buildingUnits.HasCommonBuildingUnit()
                && !_buildingUnits.NotCommonBuildingUnits().Any())
            {
                ApplyChange(new BuildingUnitWasRemovedV2(
                    BuildingPersistentLocalId,
                    _buildingUnits.CommonBuildingUnit().BuildingUnitPersistentLocalId));
            }

            NotRealizeOrRetireCommonBuildingUnit();
        }

        private void GuardBuildingInvalidStatuses(BuildingStatus[] invalidStatuses)
        {
            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
        }

        private void AddOrUpdateStatusCommonBuildingUnit()
        {
            GuardRemovedBuilding();
            GuardActiveBuilding();

            if (_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || !_buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            if (!_buildingUnits.HasCommonBuildingUnit())
            {
                AddCommonBuildingUnit(_addCommonBuildingUnit);
            }
            else
            {
                UpdateStatusCommonBuildingUnit();
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

        private void UpdateStatusCommonBuildingUnit()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            void CheckToUpdateCommonBuildingUnit ()
            {
                var commonBuildingUnitPosition = commonBuildingUnit.BuildingUnitPosition.Geometry != BuildingGeometry.Center
                    ? BuildingGeometry.Center
                    : null;

                if (commonBuildingUnitPosition is not null)
                {
                    ApplyChange(new BuildingUnitPositionWasCorrected(
                        BuildingPersistentLocalId,
                        commonBuildingUnit.BuildingUnitPersistentLocalId,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject,
                        commonBuildingUnitPosition));
                }
            }

            if ((BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction)
                && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CheckToUpdateCommonBuildingUnit();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CheckToUpdateCommonBuildingUnit();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
                ApplyChange(new BuildingUnitWasRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.Retired)
            {
                CheckToUpdateCommonBuildingUnit();

                ApplyChange(new BuildingUnitWasCorrectedFromRetiredToRealized(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
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

            if (commonBuildingUnit.Status == BuildingUnitStatus.Realized)
            {
                ApplyChange(new BuildingUnitWasRetiredV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }
    }
}
