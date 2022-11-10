namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;
    using Exceptions;

    public partial class BuildingUnit : Entity
    {
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

        private static List<BuildingUnitStatus> StatusesWhichCannotBeRealized => new()
        {
            BuildingUnitStatus.Retired,
            BuildingUnitStatus.NotRealized
        };

        private static List<BuildingUnitStatus> StatusesWhichCannotBeNotRealized => new()
        {
            BuildingUnitStatus.Realized,
            BuildingUnitStatus.Retired
        };

        public void Realize()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(StatusesWhichCannotBeRealized.ToArray());

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

            if (StatusesWhichCannotBeRealized.Contains(Status))
            {
                return;
            }

            Apply(new BuildingUnitWasRealizedBecauseBuildingWasRealized(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectRealizeBecauseBuildingWasCorrected()
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

        public void CorrectRealizeBuildingUnit()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Planned)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(new[]
            {
                BuildingUnitStatus.Retired,
                BuildingUnitStatus.NotRealized
            });

            Apply(new BuildingUnitWasCorrectedFromRealizedToPlanned(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void NotRealize()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.NotRealized)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(StatusesWhichCannotBeNotRealized.ToArray());

            Apply(new BuildingUnitWasNotRealizedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectNotRealize(BuildingGeometry buildingGeometry)
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Planned)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(new[]
            {
                BuildingUnitStatus.Realized,
                BuildingUnitStatus.Retired
            });

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

            if (StatusesWhichCannotBeNotRealized.Contains(Status))
            {
                return;
            }

            Apply(new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectPosition(BuildingUnitPositionGeometryMethod positionGeometryMethod, ExtendedWkbGeometry finalPosition)
        {
            GuardRemoved();
            GuardCommonUnit();
            GuardBuildingUnitInvalidStatuses(new[]
            {
                BuildingUnitStatus.NotRealized,
                BuildingUnitStatus.Retired
            });

            Apply(new BuildingUnitPositionWasCorrected(
                _buildingPersistentLocalId,
                BuildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition));
        }

        public void Retire()
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Retired)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(new[]
            {
                BuildingUnitStatus.Planned,
                BuildingUnitStatus.NotRealized
            });

            Apply(new BuildingUnitWasRetiredV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
        }

        public void CorrectRetiredBuildingUnit(BuildingGeometry buildingGeometry)
        {
            GuardRemoved();
            GuardCommonUnit();

            if (Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            GuardBuildingUnitInvalidStatuses(new[]
            {
                BuildingUnitStatus.Planned,
                BuildingUnitStatus.NotRealized
            });

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

        private void GuardBuildingUnitInvalidStatuses(BuildingUnitStatus[] invalidStatuses)
        {
            if (invalidStatuses.Contains(Status))
            {
                throw new BuildingUnitHasInvalidStatusException();
            }
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

        public void Remove()
        {
            GuardCommonUnit();

            if (IsRemoved)
            {
                return;
            }

            Apply(new BuildingUnitWasRemovedV2(_buildingPersistentLocalId, BuildingUnitPersistentLocalId));
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

        private void GuardRemoved()
        {
            if (IsRemoved)
            {
                throw new BuildingUnitIsRemovedException(BuildingUnitPersistentLocalId);
            }
        }

        private void GuardCommonUnit()
        {
            if (Function == BuildingUnitFunction.Common)
            {
                throw new BuildingUnitHasInvalidFunctionException();
            }
        }
    }
}
