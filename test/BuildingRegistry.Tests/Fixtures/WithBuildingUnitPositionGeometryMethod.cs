namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Building;

    public class WithBuildingUnitPositionGeometryMethod : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var methods = new List<BuildingUnitPositionGeometryMethod>
                {
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });

            fixture.Register(() =>
            {
                var methods = new List<BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod>
                {
                    BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });
        }
    }
}
