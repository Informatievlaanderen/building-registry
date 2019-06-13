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

    public class ImportHouseNumberPositionFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("09e6c007-a033-4c6a-8600-9395f2ae9585");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabAddressPositionId AddressPositionId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public WkbGeometry AddressPosition { get; }
        public CrabAddressNature AddressNature { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }

        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportHouseNumberPositionFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabAddressPositionId addressPositionId,
            CrabHouseNumberId houseNumberId,
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
            HouseNumberId = houseNumberId;
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
            => Deterministic.Create(Namespace, $"ImportHouseNumberPositionFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return TerrainObjectHouseNumberId;
            yield return AddressPositionId;
            yield return HouseNumberId;
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
