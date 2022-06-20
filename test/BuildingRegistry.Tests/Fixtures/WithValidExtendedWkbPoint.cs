namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;

    public class WithValidExtendedWkbPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Building.ExtendedWkbGeometry>(c => c.FromFactory(() => new Building.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
        }
    }
}
