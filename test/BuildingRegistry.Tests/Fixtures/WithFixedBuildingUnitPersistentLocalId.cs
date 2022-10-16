namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Building;
    using BuildingRegistry.Legacy;

    public class WithFixedBuildingUnitPersistentLocalId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var persistentLocalIdInt = fixture.Create<int>();

            fixture.Register(() => new BuildingUnitPersistentLocalId(persistentLocalIdInt));
            fixture.Register(() => new PersistentLocalId(persistentLocalIdInt));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(persistentLocalIdInt),
                    new ParameterSpecification(
                        typeof(int),
                        "buildingUnitPersistentLocalId")));
        }
    }
}
