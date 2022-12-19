namespace BuildingRegistry.Tests.BackOffice.Builders
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using BuildingRegistry.Legacy;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingStatus = BuildingRegistry.Legacy.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class MigrateBuildingBuilder
    {
        private readonly Fixture _fixture;
        private int? _buildingPersistentLocalId;
        private int? _buildingUnitPersistentLocalId;
        private int? _addressPersistentLocalId;

        public MigrateBuildingBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public MigrateBuildingBuilder WithBuildingPersistentLocalId(int buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;

            return this;
        }
        public MigrateBuildingBuilder WithBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = buildingUnitPersistentLocalId;

            return this;
        }
        public MigrateBuildingBuilder WithAddressPersistentLocalId(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = addressPersistentLocalId;

            return this;
        }

        public MigrateBuilding Build()
        {
            var command = new MigrateBuilding(_fixture.Create<BuildingId>(),
                new PersistentLocalId(_buildingPersistentLocalId ?? 123),
                _fixture.Create<PersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                _fixture.Create<BuildingGeometry>(),
                false,
                new List<BuildingUnit>()
                {
                    new BuildingUnit(_fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        new PersistentLocalId(_buildingUnitPersistentLocalId ?? 456),
                        BuildingUnitFunction.Unknown, BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(){new AddressPersistentLocalId(_addressPersistentLocalId ?? 789)},
                        _fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        _fixture.Create<BuildingGeometry>(),
                        false)
                },
                _fixture.Create<Provenance>()
            );

            return command;
        }
    }
}
