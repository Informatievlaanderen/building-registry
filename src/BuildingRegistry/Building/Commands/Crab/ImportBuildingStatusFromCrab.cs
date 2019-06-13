namespace BuildingRegistry.Building.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ValueObjects.Crab;

    public class ImportBuildingStatusFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("95460ee9-b9eb-42a9-94af-d6bd452b9b5f");

        public CrabBuildingStatusId BuildingStatusId { get; }
        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabBuildingStatus BuildingStatus { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportBuildingStatusFromCrab(
            CrabBuildingStatusId buildingStatusId,
            CrabTerrainObjectId terrainObjectId,
            CrabBuildingStatus buildingStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            BuildingStatusId = buildingStatusId;
            TerrainObjectId = terrainObjectId;
            BuildingStatus = buildingStatus;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"ImportBuildingStatusFromCrab-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingStatusId;
            yield return TerrainObjectId;
            yield return BuildingStatus;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
