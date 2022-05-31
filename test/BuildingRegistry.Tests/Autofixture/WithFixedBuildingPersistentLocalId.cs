namespace BuildingRegistry.Tests.Autofixture
{
    using AutoFixture;
    using Building;

    public class WithFixedBuildingPersistentLocalId : ICustomization
    {
        public void Customize(AutoFixture.IFixture fixture)
        {
            var persistentLocalIdInt = fixture.Create<int>();

            fixture.Register(() => new BuildingPersistentLocalId(persistentLocalIdInt));

            fixture.Customizations.Add(
                new AutoFixture.Kernel.FilteringSpecimenBuilder(
                    new AutoFixture.Kernel.FixedBuilder(persistentLocalIdInt),
                    new AutoFixture.Kernel.ParameterSpecification(
                        typeof(int),
                        "buildingPersistentLocalId")));
        }
    }
}
