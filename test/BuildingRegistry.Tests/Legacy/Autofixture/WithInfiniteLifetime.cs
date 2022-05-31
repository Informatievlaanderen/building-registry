namespace BuildingRegistry.Tests.Legacy.Autofixture
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
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
