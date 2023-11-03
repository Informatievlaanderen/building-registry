namespace BuildingRegistry.Tests.Extensions
{
    using System.Collections.Generic;
    using AutoFixture;
    using Building;
    using BuildingRegistry.Legacy;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class BuildingUnitBuilder
    {
        private readonly Fixture _fixture;

        private PersistentLocalId? _persistentLocalId;
        private BuildingUnitFunction? _buildingUnitFunction;
        private BuildingUnitStatus? _buildingUnitStatus;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        private BuildingUnitPosition? _buildingUnitPosition;
        private BuildingGeometry? _buildingGeometry;
        private bool _isRemoved;

        public BuildingUnitBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingUnitBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public BuildingUnitBuilder WithFunction(BuildingUnitFunction function)
        {
            _buildingUnitFunction = function;
            return this;
        }

        public BuildingUnitBuilder WithStatus(BuildingUnitStatus status)
        {
            _buildingUnitStatus = status;
            return this;
        }

        public BuildingUnitBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
            return this;
        }

        public BuildingUnitBuilder WithPosition(BuildingUnitPosition position)
        {
            _buildingUnitPosition = position;
            return this;
        }

        public BuildingUnitBuilder WithGeometry(BuildingGeometry buildingGeometry)
        {
            _buildingGeometry = buildingGeometry;
            return this;
        }

        public BuildingUnitBuilder WithIsRemoved()
        {
            _isRemoved = true;
            return this;
        }

        public BuildingUnit Build()
        {
            var buildingUnit = new BuildingUnit(
                _fixture.Create<BuildingUnitId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _buildingUnitFunction ?? _fixture.Create<BuildingUnitFunction>(),
                _buildingUnitStatus ?? _fixture.Create<BuildingUnitStatus>(),
                _addressPersistentLocalIds,
                _buildingUnitPosition ?? _fixture.Create<BuildingUnitPosition>(),
                _buildingGeometry ?? _fixture.Create<BuildingGeometry>(),
                _isRemoved);

            return buildingUnit;
        }
    }
}
