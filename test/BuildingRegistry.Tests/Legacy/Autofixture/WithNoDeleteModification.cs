namespace BuildingRegistry.Tests.Legacy.Autofixture
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;

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
