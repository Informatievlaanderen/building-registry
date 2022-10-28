namespace BuildingRegistry.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using ExtendedWkbGeometry = BuildingRegistry.Legacy.ExtendedWkbGeometry;
    using BuildingGeometryMethod = BuildingRegistry.Legacy.BuildingGeometryMethod;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;

    public class BuildingWasMigratedBuilder
    {
        private readonly Fixture _fixture;
        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingStatus? _buildingStatus;
        private bool _isBuildingRemoved = false;
        private readonly List<BuildingUnit> _buildingUnits = new();
        private BuildingRegistry.Building.BuildingGeometry? _buildingGeometry;

        public BuildingWasMigratedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingWasMigratedBuilder WithBuildingPersistentLocalId(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingStatus(BuildingStatus status)
        {
            _buildingStatus = status;
            return this;
        }

        public BuildingWasMigratedBuilder WithIsRemoved()
        {
            _isBuildingRemoved = true;
            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingStatus(string status)
        {
            _buildingStatus = BuildingStatus.Parse(status);
            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingGeometry(BuildingRegistry.Building.BuildingGeometry buildingGeometry)
        {
            if (_buildingUnits.Any())
            {
                throw new Exception("Can't add building geometry after one or more building units were added.");
            }

            _buildingGeometry = buildingGeometry;
            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingUnit(
            BuildingRegistry.Legacy.BuildingUnitStatus status,
            BuildingUnitPersistentLocalId? buildingUnitPersistentLocalId = null,
            BuildingUnitFunction? function = null,
            BuildingUnitPosition? position = null)
        {
            _buildingUnits.Add(
                    new BuildingUnit(
                        _fixture.Create<BuildingUnitId>(),
                        buildingUnitPersistentLocalId is not null
                            ? new PersistentLocalId(buildingUnitPersistentLocalId)
                            : _fixture.Create<PersistentLocalId>(),
                        function ?? BuildingUnitFunction.Unknown,
                    status,
                    new List<AddressPersistentLocalId>(),
                    position ?? _fixture.Create<BuildingUnitPosition>(),
                    _buildingGeometry is not null
                        ? new BuildingGeometry(
                            new ExtendedWkbGeometry(_buildingGeometry.Geometry.ToString()),
                            _buildingGeometry.Method == BuildingRegistry.Building.BuildingGeometryMethod.Outlined
                                ? BuildingGeometryMethod.Outlined
                                : BuildingGeometryMethod.MeasuredByGrb)
                        : _fixture.Create<BuildingGeometry>(),
                    isRemoved: false));

            return this;
        }

        public BuildingWasMigrated Build()
        {
            var buildingWasMigrated = new BuildingWasMigrated(
                _fixture.Create<BuildingId>(),
                _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                _buildingStatus ?? BuildingStatus.Planned,
                _buildingGeometry ?? _fixture.Create<BuildingRegistry.Building.BuildingGeometry>(),
                _isBuildingRemoved,
                _buildingUnits);
            ((ISetProvenance)buildingWasMigrated).SetProvenance(_fixture.Create<Provenance>());

            return buildingWasMigrated;
        }
    }
}
