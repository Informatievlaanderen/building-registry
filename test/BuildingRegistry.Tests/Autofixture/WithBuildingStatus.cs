namespace BuildingRegistry.Tests.Autofixture
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Building;

    public class WithBuildingStatus : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var statuses = new List<BuildingStatus>
                {
                    BuildingStatus.Planned,
                    BuildingStatus.Realized,
                    BuildingStatus.Retired,
                    BuildingStatus.NotRealized,
                };

                return statuses[new Random(fixture.Create<int>()).Next(0, statuses.Count - 1)];
            });
        }
    }

    public class WithBuildingRetiredStatus : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var statuses = new List<BuildingStatus>
                {
                    BuildingStatus.Retired,
                    BuildingStatus.NotRealized,
                };

                return statuses[new Random(fixture.Create<int>()).Next(0, statuses.Count - 1)];
            });
        }
    }
}
