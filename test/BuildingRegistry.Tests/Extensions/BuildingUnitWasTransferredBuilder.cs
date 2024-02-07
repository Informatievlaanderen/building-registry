namespace BuildingRegistry.Tests.Extensions
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public class BuildingUnitWasTransferredBuilder
    {
        private readonly Fixture _fixture;

        private int? _buildingPersistentLocalId;
        private int? _buildingUnitPersistentLocalId;
        private int? _sourceBuildingPersistentLocalId;

        public BuildingUnitWasTransferredBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingUnitWasTransferredBuilder WithBuildingPersistentLocalId(int buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            return this;
        }

        public BuildingUnitWasTransferredBuilder WithBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            return this;
        }

        public BuildingUnitWasTransferredBuilder WithSourceBuildingPersistentLocalId(int sourceBuildingPersistentLocalId)
        {
            _sourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
            return this;
        }

        // public BuildingUnitWasTransferred Build()
        // {
        //     var buildingPersistentLocalId = new BuildingPersistentLocalId(_buildingPersistentLocalId ?? _fixture.Create<int>());
        //     var buildingUnitPosition = _fixture.Create<BuildingUnitPosition>();
        //
        //     var @event = new BuildingUnitWasTransferred(
        //         buildingPersistentLocalId,
        //         BuildingUnit.Transfer(_ => { },
        //             buildingPersistentLocalId,
        //             new BuildingUnitPersistentLocalId(_buildingUnitPersistentLocalId ?? _fixture.Create<int>()),
        //             BuildingUnitFunction.Unknown,
        //             BuildingUnitStatus.Realized,
        //             _fixture.Create<List<AddressPersistentLocalId>>(),
        //             buildingUnitPosition,
        //             false),
        //         new BuildingPersistentLocalId(_sourceBuildingPersistentLocalId ?? _fixture.Create<int>()),
        //         buildingUnitPosition);
        //
        //     ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());
        //
        //     return @event;
        // }
    }
}
