namespace BuildingRegistry.ValueObjects
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;

    public class BuildingId : GuidValueObject<BuildingId>
    {
        public BuildingId(Guid buildingId) : base(buildingId) { }

        public static BuildingId CreateFor(CrabTerrainObjectId crabTerrainObjectId)
            => new BuildingId(crabTerrainObjectId.CreateDeterministicId());
    }
}
