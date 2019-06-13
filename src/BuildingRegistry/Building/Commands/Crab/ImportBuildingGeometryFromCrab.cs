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

    public class ImportBuildingGeometryFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("f2d68df2-c23b-4db7-9f31-34dd45eca87b");

        public CrabBuildingGeometryId BuildingGeometryId { get; }
        public CrabTerrainObjectId TerrainObjectId { get; }
        public WkbGeometry BuildingGeometry { get; }
        public CrabBuildingGeometryMethod BuildingGeometryMethod { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportBuildingGeometryFromCrab(
            CrabBuildingGeometryId buildingGeometryId,
            CrabTerrainObjectId terrainObjectId,
            WkbGeometry buildingGeometry,
            CrabBuildingGeometryMethod buildingGeometryMethod,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            BuildingGeometryId = buildingGeometryId;
            TerrainObjectId = terrainObjectId;
            BuildingGeometry = buildingGeometry;
            BuildingGeometryMethod = buildingGeometryMethod;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"ImportBuildingGeometryFromCrab-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingGeometryId;
            yield return TerrainObjectId;
            yield return BuildingGeometry;
            yield return BuildingGeometryMethod;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
