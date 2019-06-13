namespace BuildingRegistry.Tests.Autofixture
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;

    public class WithNoDeleteModification : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var crabModification = fixture.Create<Generator<CrabModification>>()
                .FirstOrDefault(modification => modification != CrabModification.Delete);

            fixture.Register(() => crabModification);
        }
    }
}
