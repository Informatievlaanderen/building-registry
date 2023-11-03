namespace BuildingRegistry.Tests.Extensions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public class BuildingUnitWasPlannedV2Builder
    {
        private readonly Fixture _fixture;

        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingUnitPersistentLocalId? _buildingUnitPersistentLocalId;
        private BuildingUnitPositionGeometryMethod? _positionGeometryMethod;
        private ExtendedWkbGeometry? _extendedWkbGeometry;
        private BuildingUnitFunction? _buildingUnitFunction;
        private bool _hasDeviation;

        public BuildingUnitWasPlannedV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingUnitWasPlannedV2Builder WithBuildingPersistentLocalId(int buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = new BuildingPersistentLocalId(buildingPersistentLocalId);
            return this;
        }

        public BuildingUnitWasPlannedV2Builder WithBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            return this;
        }

        public BuildingUnitWasPlannedV2Builder WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod buildingUnitPositionGeometryMethod)
        {
            _positionGeometryMethod = buildingUnitPositionGeometryMethod;
            return this;
        }

        public BuildingUnitWasPlannedV2Builder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;
            return this;
        }

        public BuildingUnitWasPlannedV2Builder WithFunction(BuildingUnitFunction function)
        {
            _buildingUnitFunction = function;
            return this;
        }

        public BuildingUnitWasPlannedV2Builder WithDeviation()
        {
            _hasDeviation = true;
            return this;
        }

        public BuildingUnitWasPlannedV2 Build()
        {
            var buildingUnit = new BuildingUnitWasPlannedV2(
                _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>(),
                _buildingUnitPersistentLocalId ?? _fixture.Create<BuildingUnitPersistentLocalId>(),
                _positionGeometryMethod ?? _fixture.Create<BuildingUnitPositionGeometryMethod>(),
                _extendedWkbGeometry ?? _fixture.Create<ExtendedWkbGeometry>(),
                _buildingUnitFunction ?? _fixture.Create<BuildingUnitFunction>(),
                _hasDeviation
            );

            ((ISetProvenance)buildingUnit).SetProvenance(_fixture.Create<Provenance>());

            return buildingUnit;
        }
    }
}
