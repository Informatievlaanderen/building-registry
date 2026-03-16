namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;

    public class WithValidExtendedWkbPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var validPolygon = GeometryHelper.ValidPolygon;
            validPolygon.WithSrid(ExtendedWkbGeometry.SridLambert72);
            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(WkbWriter.Instance.Write(validPolygon))));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(WkbWriter.Instance.Write(validPolygon))));
        }
    }

    public class WithValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var point = GeometryHelper.ValidPointInPolygon;
            point.WithSrid(ExtendedWkbGeometry.SridLambert72);

            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(WkbWriter.Instance.Write(point))));
        }
    }

    public class WithTooSmallBuilding : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var smallPolygon = GeometryHelper.TooSmallPolygon;
            smallPolygon.WithSrid(ExtendedWkbGeometry.SridLambert72);

            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(WkbWriter.Instance.Write(smallPolygon))));
        }
    }
}
