namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Legacy.Crab;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportSubaddressFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("84418e05-8aeb-4c05-926c-588e124b5cf4");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public BoxNumber BoxNumber { get; }
        public CrabBoxNumberType BoxNumberType { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportSubaddressFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification, CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            SubaddressId = subaddressId;
            HouseNumberId = houseNumberId;
            BoxNumber = boxNumber;
            BoxNumberType = boxNumberType;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return TerrainObjectHouseNumberId;
            yield return SubaddressId;
            yield return HouseNumberId;
            yield return BoxNumber;
            yield return BoxNumberType;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
