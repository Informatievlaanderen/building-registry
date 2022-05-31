namespace BuildingRegistry.Tests.Autofixture
{
    using System;
    using AutoFixture;
    using Building;

    public class WithFixedBuildingId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<BuildingId>(c => c.FromFactory(() => new BuildingId(Guid.NewGuid())));
        }
    }
}
