namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RequestPersistentLocalIdsForCrabTerrainObjectId
    {
        private static readonly Guid Namespace = new Guid("d238d19f-7a23-4376-8a32-803e1aaabf6b");

        public CrabTerrainObjectId TerrainObjectId { get; }

        public RequestPersistentLocalIdsForCrabTerrainObjectId(CrabTerrainObjectId terrainObjectId)
        {
            TerrainObjectId = terrainObjectId;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"RequestPersistentLocalIdsForCrabTerrainObjectId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return DateTimeOffset.Now; // must be unique due to idempotency => any time a new building unit can be added and must get a new persistent local id
        }
    }
}
