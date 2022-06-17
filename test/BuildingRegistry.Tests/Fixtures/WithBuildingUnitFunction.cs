namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Building;

    public class WithBuildingUnitFunction : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var methods = new List<BuildingUnitFunction>
                {
                    BuildingUnitFunction.Unknown,
                    BuildingUnitFunction.Common,
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });

            fixture.Register(() =>
            {
                var methods = new List<BuildingRegistry.Legacy.BuildingUnitFunction>
                {
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });
        }
    }
}
