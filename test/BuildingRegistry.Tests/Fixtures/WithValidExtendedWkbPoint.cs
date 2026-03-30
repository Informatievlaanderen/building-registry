namespace BuildingRegistry.Tests.Fixtures
{
    using AutoFixture;
    using Building;

    public class WithValidExtendedWkbPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(() => new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPointInPolygon))));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPointInPolygon))));
        }
    }
}
