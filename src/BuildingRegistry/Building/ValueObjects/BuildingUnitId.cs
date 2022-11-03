namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class BuildingUnitId : GuidValueObject<BuildingUnitId>
    {
        public BuildingUnitId(Guid buildingUnitId) : base(buildingUnitId) { }
    }
}
