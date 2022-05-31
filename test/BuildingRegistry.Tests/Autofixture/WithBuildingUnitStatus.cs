namespace BuildingRegistry.Tests.Autofixture
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
        }
    }
}
