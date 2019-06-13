namespace BuildingRegistry.Building.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ValueObjects.Crab;

    public class ImportHouseNumberStatusFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("3f115c10-df93-4b9b-9262-6f36e9a1b6d8");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabHouseNumberStatusId HouseNumberStatusId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public CrabAddressStatus AddressStatus { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportHouseNumberStatusFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            HouseNumberStatusId = houseNumberStatusId;
            HouseNumberId = houseNumberId;
            AddressStatus = addressStatus;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportHouseNumberStatusFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return TerrainObjectHouseNumberId;
            yield return HouseNumberStatusId;
            yield return HouseNumberId;
            yield return AddressStatus;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
