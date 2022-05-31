namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class FixGrar1359
    {
        private static readonly Guid Namespace = new Guid("199ffef9-9afb-41c3-9d5f-0d80b6eaa21b");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public IEnumerable<ImportSubaddressFromCrab> SubaddressCommandsFromCrab { get; }

        public FixGrar1359(
            IEnumerable<ImportSubaddressFromCrab> subaddressCommandsFromCrab,
            CrabTerrainObjectId terrainObjectId)
        {
            SubaddressCommandsFromCrab = subaddressCommandsFromCrab;
            TerrainObjectId = terrainObjectId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"FixGrar1359-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return string.Join(",", SubaddressCommandsFromCrab.Select(x => x.CreateCommandId()));
        }
    }
}
