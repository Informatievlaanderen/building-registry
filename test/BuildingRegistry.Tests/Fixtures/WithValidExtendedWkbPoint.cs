namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using Building;

    public class WithValidExtendedWkbPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
        }
    }
}
