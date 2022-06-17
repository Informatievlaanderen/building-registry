namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Building;

    public class WithBuildingGeometryMethod : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var methods = new List<BuildingGeometryMethod>
                {
                    BuildingGeometryMethod.MeasuredByGrb,
                    BuildingGeometryMethod.Outlined
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });
        }
    }
}
