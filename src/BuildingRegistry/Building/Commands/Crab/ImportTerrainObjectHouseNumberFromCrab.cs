namespace BuildingRegistry.Building.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportTerrainObjectHouseNumberFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("98415f15-81c2-484a-a151-0c6a674974fd");

        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportTerrainObjectHouseNumberFromCrab(
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabTerrainObjectId terrainObjectId,
            CrabHouseNumberId houseNumberId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            TerrainObjectId = terrainObjectId;
            HouseNumberId = houseNumberId;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"ImportTerrainObjectHouseNumberFromCrab-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectHouseNumberId;
            yield return TerrainObjectId;
            yield return HouseNumberId;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
