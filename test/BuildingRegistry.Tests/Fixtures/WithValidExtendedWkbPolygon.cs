namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using Building;

    public class WithValidExtendedWkbPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var validPolygon = GeometryHelper.ValidPolygon;
            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(validPolygon.AsBinary())));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(validPolygon.AsBinary())));
        }
    }

    public class WithValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
        }
    }
}
