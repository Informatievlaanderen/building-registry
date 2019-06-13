namespace BuildingRegistry.Tests.Autofixture
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using NodaTime;

    public class WithInfiniteLifetime : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CrabLifetime>(c => c.FromFactory(
                () => new CrabLifetime(fixture.Create<LocalDateTime>(), null)));
        }
    }
}
