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
                var statusses = new List<BuildingStatus>
                {
                BuildingStatus.Planned,
                BuildingStatus.Retired,
                BuildingStatus.Retired,
                BuildingStatus.NotRealized,
                };

                return statusses[new Random(fixture.Create<int>()).Next(0, statusses.Count - 1)];
            });
        }
    }
}
