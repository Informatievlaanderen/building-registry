namespace BuildingRegistry.Building.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RequestOsloIdsForCrabTerrainObjectId
    {
        private static readonly Guid Namespace = new Guid("d238d19f-7a23-4376-8a32-803e1aaabf6b");

        public CrabTerrainObjectId TerrainObjectId { get; }

        public RequestOsloIdsForCrabTerrainObjectId(CrabTerrainObjectId terrainObjectId)
        {
            TerrainObjectId = terrainObjectId;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"RequestOsloIdsForCrabTerrainObjectId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return DateTimeOffset.Now; // must be unique due to idempotency => any time a new building unit can be added and must get a new oslo id
        }
    }
}
