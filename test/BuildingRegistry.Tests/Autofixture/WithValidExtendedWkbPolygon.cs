namespace BuildingRegistry.Tests.Autofixture
{
    using AutoFixture;

    public class WithValidExtendedWkbPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Building.ExtendedWkbGeometry>(c => c.FromFactory(() => new Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
            fixture.Customize<BuildingRegistry.Legacy.ExtendedWkbGeometry>(c => c.FromFactory(() => new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
        }
    }
}
