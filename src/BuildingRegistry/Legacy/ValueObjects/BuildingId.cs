namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    public class BuildingId : GuidValueObject<BuildingId>
    {
        public BuildingId([JsonProperty("value")] Guid buildingId) : base(buildingId) { }

        public static BuildingId CreateFor(CrabTerrainObjectId crabTerrainObjectId)
            => new BuildingId(crabTerrainObjectId.CreateDeterministicId());
    }
}
