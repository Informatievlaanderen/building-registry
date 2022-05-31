namespace BuildingRegistry.Tests.Legacy.Autofixture
{
    using AutoFixture;
    using BuildingRegistry.Legacy;

    public class WithValidPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
        }
    }

    public class WithValidPolygonOutsideValidPointBoundary : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
        }
    }

    public class WithValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
        }
    }

    public class WithOtherValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary())));
        }
    }

    public class WithInvalidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary())));
        }
    }
}
