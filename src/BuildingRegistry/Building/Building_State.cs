namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class Building
    {
        private IBuildingEvent? _lastEvent;

        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;

        private readonly BuildingUnits _buildingUnits = new BuildingUnits();

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; private set; }
        public BuildingStatus BuildingStatus { get; private set; }
        public BuildingGeometry BuildingGeometry { get; private set; }
        public bool IsRemoved { get; private set; }

        public IReadOnlyList<BuildingUnit> BuildingUnits => _buildingUnits;

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();

        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        internal Building(ISnapshotStrategy snapshotStrategy)
            : this()
        {
            Strategy = snapshotStrategy;
        }

        private Building()
        {
            Register<BuildingWasMigrated>(When);
            Register<BuildingWasPlannedV2>(When);
            Register<BuildingBecameUnderConstructionV2>(When);
            Register<BuildingWasRealizedV2>(When);
            Register<BuildingWasNotRealizedV2>(When);
            Register<BuildingWasRemovedV2>(When);
            Register<BuildingWasCorrectedFromUnderConstructionToPlanned>(When);
            Register<BuildingWasCorrectedFromRealizedToUnderConstruction>(When);
            Register<BuildingWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingOutlineWasChanged>(When);
            Register<UnplannedBuildingWasRealizedAndMeasured>(When);
            Register<BuildingWasDemolished>(When);
            Register<BuildingWasMeasured>(When);
            Register<BuildingMeasurementWasCorrected>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<CommonBuildingUnitWasAddedV2>(When);
            Register<BuildingUnitWasRealizedV2>(When);
            Register<BuildingUnitWasRealizedBecauseBuildingWasRealized>(When);
            Register<BuildingUnitWasNotRealizedV2>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(When);
            Register<BuildingUnitWasRetiredV2>(When);
            Register<BuildingUnitWasRemovedV2>(When);
            Register<BuildingUnitWasRemovedBecauseBuildingWasRemoved>(When);
            Register<BuildingUnitWasRegularized>(When);
            Register<BuildingUnitWasDeregulated>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(When);
            Register<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRetiredToRealized>(When);
            Register<BuildingUnitRemovalWasCorrected>(When);
            Register<BuildingUnitPositionWasCorrected>(When);
            Register<BuildingUnitRegularizationWasCorrected>(When);
            Register<BuildingUnitDeregulationWasCorrected>(When);
            Register<BuildingUnitAddressWasAttachedV2>(When);
            Register<BuildingUnitAddressWasDetachedV2>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(When);
            Register<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>(When);
            Register<BuildingUnitWasRetiredBecauseBuildingWasDemolished>(When);
            Register<BuildingMeasurementWasChanged>(When);

            Register<BuildingMergerWasRealized>(When);
            Register<BuildingUnitWasTransferred>(When);

            Register<BuildingWasMerged>(When);
            Register<BuildingUnitWasMoved>(When);

            Register<BuildingSnapshot>(When);
        }

        private void When(BuildingWasMigrated @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingStatus = BuildingStatus.Parse(@event.BuildingStatus);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Parse(@event.GeometryMethod));
            IsRemoved = @event.IsRemoved;

            var nonCommonBuildingUnits = @event.BuildingUnits.Where(x => x.Function != BuildingUnitFunction.Common);
            var commonBuildingUnits = @event.BuildingUnits.Where(x => x.Function == BuildingUnitFunction.Common).ToList();

            var commonBuildingUnit = commonBuildingUnits.FirstOrDefault();
            if (commonBuildingUnits.Count > 1)
            {
                var nonRemovedCommonBuildingUnits = commonBuildingUnits.Where(x => !x.IsRemoved).ToList();
                commonBuildingUnit = nonRemovedCommonBuildingUnits.FirstOrDefault();

                if (nonRemovedCommonBuildingUnits.Count > 1)
                {
                    var activeBuildingUnitStatuses = new[] { BuildingStatus.Planned, BuildingStatus.Realized };

                    var plannedOrRealizedCommonBuildingUnits = nonRemovedCommonBuildingUnits
                        .Where(x => activeBuildingUnitStatuses.Contains(BuildingStatus.Parse(x.Status)))
                        .ToList();
                    commonBuildingUnit = plannedOrRealizedCommonBuildingUnits.FirstOrDefault();

                    if (plannedOrRealizedCommonBuildingUnits.Count > 1)
                    {
                        throw new InvalidOperationException(
                            $"Building {@event.BuildingPersistentLocalId} contains more than one active common building unit.");
                    }

                    if (commonBuildingUnit is null)
                    {
                        var notRealizedOrRetiredCommonBuildingUnits = nonRemovedCommonBuildingUnits
                            .Where(x =>!activeBuildingUnitStatuses.Contains(BuildingStatus.Parse(x.Status)))
                            .ToList();

                        commonBuildingUnit = notRealizedOrRetiredCommonBuildingUnits
                            .OrderByDescending(x => x.BuildingUnitPersistentLocalId)
                            .FirstOrDefault();
                    }
                }
                else if (commonBuildingUnit is null)
                {
                    commonBuildingUnit = commonBuildingUnits
                        .OrderByDescending(x => x.BuildingUnitPersistentLocalId)
                        .FirstOrDefault();
                }
            }

            var buildingUnits = commonBuildingUnit is not null
                ? nonCommonBuildingUnits.Concat(new[] { commonBuildingUnit })
                : nonCommonBuildingUnits;

            foreach (var buildingUnit in buildingUnits)
            {
                var newBuildingUnit = BuildingUnit.Migrate(
                    ApplyChange,
                    BuildingPersistentLocalId,
                    new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
                    BuildingUnitFunction.Parse(buildingUnit.Function),
                    BuildingUnitStatus.Parse(buildingUnit.Status),
                    buildingUnit.AddressPersistentLocalIds
                        .Distinct()
                        .ToList()
                        .ConvertAll(x => new AddressPersistentLocalId(x)),
                    new BuildingUnitPosition(
                        new ExtendedWkbGeometry(buildingUnit.ExtendedWkbGeometry),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)),
                    buildingUnit.IsRemoved);

                newBuildingUnit.Route(@event);
                _buildingUnits.Add(newBuildingUnit);
            }

            _lastEvent = @event;
        }

        private void When(BuildingWasPlannedV2 @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingBecameUnderConstructionV2 @event)
        {
            BuildingStatus = BuildingStatus.UnderConstruction;

            _lastEvent = @event;
        }

        private void When(BuildingWasRealizedV2 @event)
        {
            BuildingStatus = BuildingStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingWasNotRealizedV2 @event)
        {
            BuildingStatus = BuildingStatus.NotRealized;

            _lastEvent = @event;
        }

        private void When(BuildingWasRemovedV2 @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromUnderConstructionToPlanned @event)
        {
            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromRealizedToUnderConstruction @event)
        {
            BuildingStatus = BuildingStatus.UnderConstruction;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromNotRealizedToPlanned @event)
        {
            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingOutlineWasChanged @event)
        {
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuilding),
                BuildingGeometryMethod.Outlined);

            foreach (var buildingUnitPersistentLocalId in @event.BuildingUnitPersistentLocalIds)
            {
                var buildingUnit = BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                buildingUnit.Route(@event);
            }

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasPlannedV2 @event)
        {
            var newBuildingUnit = new BuildingUnit(ApplyChange);
            newBuildingUnit.Route(@event);
            _buildingUnits.Add(newBuildingUnit);

            _lastEvent = @event;
        }

        private void When(CommonBuildingUnitWasAddedV2 @event)
        {
            var commonBuildingUnit = new BuildingUnit(ApplyChange);
            commonBuildingUnit.Route(@event);
            _buildingUnits.Add(commonBuildingUnit);

            _lastEvent = @event;
        }

        private void When(UnplannedBuildingWasRealizedAndMeasured @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.MeasuredByGrb);

            BuildingStatus = BuildingStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingWasMeasured @event)
        {
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuilding),
                BuildingGeometryMethod.MeasuredByGrb);

            foreach (var buildingUnitPersistentLocalId in @event.BuildingUnitPersistentLocalIds.Concat(@event
                         .BuildingUnitPersistentLocalIdsWhichBecameDerived))
            {
                var buildingUnit = BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                buildingUnit.Route(@event);
            }

            _lastEvent = @event;
        }

        private void When(BuildingMeasurementWasCorrected @event)
        {
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuilding),
                BuildingGeometryMethod.MeasuredByGrb);

            foreach (var buildingUnitPersistentLocalId in @event.BuildingUnitPersistentLocalIds.Concat(@event
                         .BuildingUnitPersistentLocalIdsWhichBecameDerived))
            {
                var buildingUnit = BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                buildingUnit.Route(@event);
            }

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRealizedBecauseBuildingWasRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasNotRealizedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRetiredV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRemovedV2 @event)
        {
            RouteToBuildingUnit(@event);

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRemovedBecauseBuildingWasRemoved @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRegularized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasDeregulated @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRealizedToPlanned @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromNotRealizedToPlanned @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRetiredToRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitRemovalWasCorrected @event)
        {
            RouteToBuildingUnit(@event);

            _lastEvent = @event;
        }

        private void When(BuildingUnitPositionWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitRegularizationWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitDeregulationWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasAttachedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRejected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRetired @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRemoved @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed @event) => RouteToBuildingUnit(@event);

        private void When(BuildingWasDemolished @event)
        {
            BuildingStatus = BuildingStatus.Retired;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasNotRealizedBecauseBuildingWasDemolished @event) => RouteToBuildingUnit(@event);
        private void When(BuildingUnitWasRetiredBecauseBuildingWasDemolished @event) => RouteToBuildingUnit(@event);

        private void When(BuildingMeasurementWasChanged @event)
        {
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuilding),
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingUnitPersistentLocalIds =
                @event.BuildingUnitPersistentLocalIds.Concat(@event.BuildingUnitPersistentLocalIdsWhichBecameDerived);

            foreach (var buildingUnitPersistentLocalId in buildingUnitPersistentLocalIds)
            {
                var buildingUnit = BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                buildingUnit.Route(@event);
            }

            _lastEvent = @event;
        }

        private void When(BuildingMergerWasRealized @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.MeasuredByGrb);

            BuildingStatus = BuildingStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasTransferred @event)
        {
            var transferredBuildingUnit = BuildingUnit.Transfer(
                ApplyChange,
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                BuildingUnitFunction.Parse(@event.Function),
                BuildingUnitStatus.Parse(@event.Status),
                @event.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)).ToList(),
                new BuildingUnitPosition(
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                    BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod)),
                @event.HasDeviation);

            transferredBuildingUnit.Route(@event);
            _buildingUnits.Add(transferredBuildingUnit);

            _lastEvent = @event;
        }

        private void When(BuildingWasMerged @event)
        {
            BuildingStatus = BuildingStatus.Retired;
            _lastEvent = @event;
        }

        private void When(BuildingUnitWasMoved @event)
        {
            var buildingUnit =
                BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == @event.BuildingUnitPersistentLocalId);
            _buildingUnits.Remove(buildingUnit);
            _lastEvent = @event;
        }

        private void RouteToBuildingUnit<TEvent>(TEvent @event)
            where TEvent : IBuildingEvent, IHasBuildingUnitPersistentLocalId
        {
            _buildingUnits
                .GetByPersistentLocalId(new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId))
                .Route(@event);
        }

        private void When(BuildingSnapshot @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingStatus = BuildingStatus.Parse(@event.BuildingStatus);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Parse(@event.GeometryMethod));
            IsRemoved = @event.IsRemoved;

            foreach (var buildingUnitData in @event.BuildingUnits)
            {
                var buildingUnit = new BuildingUnit(ApplyChange);
                buildingUnit.RestoreSnapshot(BuildingPersistentLocalId, buildingUnitData);

                _buildingUnits.Add(buildingUnit);
            }

            _lastSnapshotEventHash = @event.LastEventHash;
            _lastSnapshotProvenance = @event.LastProvenanceData;
        }
    }
}
