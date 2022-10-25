namespace BuildingRegistry.Tests.Extensions
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class BuildingWasMigratedBuilder
    {
        private readonly Fixture _fixture;
        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingStatus? _buildingStatus;
        private readonly List<BuildingUnit> _buildingUnits = new List<BuildingUnit>();
        
        public BuildingWasMigratedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingWasMigratedBuilder WithBuildingStatus(BuildingStatus status)
        {
            _buildingStatus = status;

            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingStatus(string status)
        {
            _buildingStatus = BuildingStatus.Parse(status);

            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingUnit(
            BuildingRegistry.Legacy.BuildingUnitStatus status,
            BuildingUnitPersistentLocalId? buildingUnitPersistentLocalId = null,
            BuildingRegistry.Legacy.BuildingUnitFunction? function = null)
        {
            _buildingUnits.Add(
                    new BuildingUnit(
                        _fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        buildingUnitPersistentLocalId is not null
                            ? new PersistentLocalId(buildingUnitPersistentLocalId)
                            : _fixture.Create<PersistentLocalId>(),
                        function ?? BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                    status,
                    new List<AddressPersistentLocalId>(),
                    _fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                    _fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                    isRemoved: false));

            return this;
        }

        public BuildingWasMigratedBuilder WithBuildingPersistentLocalId(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;

            return this;
        }

        public BuildingWasMigrated Build()
        {
            var buildingWasMigrated = new BuildingWasMigrated(
                _fixture.Create<BuildingRegistry.Building.BuildingId>(),
                _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                _buildingStatus ?? BuildingStatus.Planned,
                _fixture.Create<BuildingRegistry.Building.BuildingGeometry>(),
                isRemoved: false,
                _buildingUnits);
            ((ISetProvenance)buildingWasMigrated).SetProvenance(_fixture.Create<Provenance>());

            return buildingWasMigrated;
        }
    }
}
