namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Building;

    public class WithFixedAddressPersistentLocalId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var persistentLocalIdInt = fixture.Create<int>();

            fixture.Register(() => new AddressPersistentLocalId(persistentLocalIdInt));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(persistentLocalIdInt),
                    new ParameterSpecification(
                        typeof(int),
                        "addressPersistentLocalId")));
        }
    }
}
