namespace BuildingRegistry.Building.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ValueObjects;
    using ValueObjects.Crab;

    public class ImportSubaddressPositionFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("eac3cd00-3d4c-464e-89e9-e6b7f412e362");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabAddressPositionId AddressPositionId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public WkbGeometry AddressPosition { get; }
        public CrabAddressNature AddressNature { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportSubaddressPositionFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabAddressPositionId addressPositionId,
            CrabSubaddressId subaddressId,
            WkbGeometry addressPosition,
            CrabAddressNature addressNature,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            AddressPositionId = addressPositionId;
            SubaddressId = subaddressId;
            AddressPosition = addressPosition;
            AddressNature = addressNature;
            AddressPositionOrigin = addressPositionOrigin;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressPositionFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return TerrainObjectHouseNumberId;
            yield return AddressPositionId;
            yield return SubaddressId;
            yield return AddressPosition;
            yield return AddressNature;
            yield return AddressPositionOrigin;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
