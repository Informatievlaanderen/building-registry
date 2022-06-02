namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingId : GuidValueObject<BuildingId>
    {
        public BuildingId(Guid buildingId) : base(buildingId) { }
    }
}
