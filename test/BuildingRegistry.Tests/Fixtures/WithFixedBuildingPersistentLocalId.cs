namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Building;
    using BuildingRegistry.Legacy;

    public class WithFixedBuildingPersistentLocalId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var persistentLocalIdInt = fixture.Create<int>();

            fixture.Register(() => new BuildingPersistentLocalId(persistentLocalIdInt));
            fixture.Register(() => new PersistentLocalId(persistentLocalIdInt));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(persistentLocalIdInt),
                    new ParameterSpecification(
                        typeof(int),
                        "buildingPersistentLocalId")));
        }
    }
}
