namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using Building;

    public class WithFixedBuildingUnitPersistentLocalId : ICustomization
    {
        public void Customize(AutoFixture.IFixture fixture)
        {
            var persistentLocalIdInt = fixture.Create<int>();

            fixture.Register(() => new BuildingUnitPersistentLocalId(persistentLocalIdInt));
            fixture.Register(() => new BuildingRegistry.Legacy.PersistentLocalId(persistentLocalIdInt));

            fixture.Customizations.Add(
                new AutoFixture.Kernel.FilteringSpecimenBuilder(
                    new AutoFixture.Kernel.FixedBuilder(persistentLocalIdInt),
                    new AutoFixture.Kernel.ParameterSpecification(
                        typeof(int),
                        "buildingUnitPersistentLocalId")));
        }
    }
}
