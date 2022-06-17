namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Building;

    public class WithBuildingUnitStatus : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var methods = new List<BuildingUnitStatus>
                {
                    BuildingUnitStatus.Planned,
                    BuildingUnitStatus.Realized,
                    BuildingUnitStatus.Retired,
                    BuildingUnitStatus.NotRealized
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });

            fixture.Register(() =>
            {
                var methods = new List<BuildingRegistry.Legacy.BuildingUnitStatus>
                {
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized
                };

                return methods[new Random(fixture.Create<int>()).Next(0, methods.Count - 1)];
            });
        }
    }
}
