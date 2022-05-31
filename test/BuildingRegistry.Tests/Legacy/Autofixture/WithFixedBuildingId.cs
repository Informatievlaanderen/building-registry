namespace BuildingRegistry.Tests.Legacy.Autofixture
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;

    public class WithFixedBuildingId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var houseNumberId = fixture.Create<int>();
            fixture.Customize<CrabTerrainObjectId>(c => c.FromFactory(() => new CrabTerrainObjectId(houseNumberId)));
            fixture.Customize<BuildingId>(c => c.FromFactory(() => BuildingId.CreateFor(fixture.Create<CrabTerrainObjectId>())));
        }
    }

    public class WithFixedBuildingUnitIdFromHouseNumber : ICustomization
    {
        private readonly int? _terrainObjectId;
        private readonly int? _terrainObjectHouseNumberId;

        public WithFixedBuildingUnitIdFromHouseNumber(int? terrainObjectId = null, int? terrainObjectHouseNumberId = null)
        {
            _terrainObjectHouseNumberId = terrainObjectHouseNumberId;
            _terrainObjectId = terrainObjectId;
        }

        public void Customize(IFixture fixture)
        {
            var terrainObjectId = _terrainObjectId??fixture.Create<int>();
            var terrainObjectHouseNumberId = _terrainObjectHouseNumberId??fixture.Create<int>();
            var crabTerrainObjectId = new CrabTerrainObjectId(terrainObjectId);
            fixture.Customize<CrabTerrainObjectId>(c => c.FromFactory(() => crabTerrainObjectId));
            fixture.Customize<BuildingId>(c => c.FromFactory(() => BuildingId.CreateFor(fixture.Create<CrabTerrainObjectId>())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(crabTerrainObjectId.CreateDeterministicId()),
                    new ParameterSpecification(
                        typeof(Guid),
                        "buildingId")));

            var crabTerrainObjectHouseNumberId = new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId);
            var houseNumberId = fixture.Create<CrabHouseNumberId>();

            fixture.Customize<CrabTerrainObjectHouseNumberId>(c => c.FromFactory(() => crabTerrainObjectHouseNumberId));
            fixture.Customize<CrabHouseNumberId>(c => c.FromFactory(() => houseNumberId));
            var buildingUnitKey = BuildingUnitKey.Create(crabTerrainObjectId, crabTerrainObjectHouseNumberId);
            fixture.Customize<BuildingUnitKey>(c => c.FromFactory(() => buildingUnitKey));

            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(buildingUnitId),
                    new ParameterSpecification(
                        typeof(Guid),
                        "buildingUnitId")));


            fixture.Register(() => buildingUnitId);
        }
    }

    public class WithFixedBuildingUnitIdFromSubaddress : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var terrainObjectId = fixture.Create<int>();
            var terrainObjectHouseNumberId = fixture.Create<int>();
            var crabTerrainObjectId = new CrabTerrainObjectId(terrainObjectId);
            fixture.Customize<CrabTerrainObjectId>(c => c.FromFactory(() => crabTerrainObjectId));
            fixture.Customize<BuildingId>(c => c.FromFactory(() => BuildingId.CreateFor(fixture.Create<CrabTerrainObjectId>())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(crabTerrainObjectId.CreateDeterministicId()),
                    new ParameterSpecification(
                        typeof(Guid),
                        "buildingId")));

            var crabTerrainObjectHouseNumberId = new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId);
            var subaddressId =  fixture.Create<CrabSubaddressId>();

            fixture.Customize<CrabTerrainObjectHouseNumberId>(c => c.FromFactory(() => crabTerrainObjectHouseNumberId));
            fixture.Customize<CrabSubaddressId>(c => c.FromFactory(() => subaddressId));
            var buildingUnitKey = BuildingUnitKey.Create(crabTerrainObjectId, crabTerrainObjectHouseNumberId, subaddressId);
            fixture.Customize<BuildingUnitKey>(c => c.FromFactory(() => buildingUnitKey));

            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);
            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(buildingUnitId),
                    new ParameterSpecification(
                        typeof(Guid),
                        "buildingUnitId")));

            fixture.Register(() => buildingUnitId);
        }
    }
}
