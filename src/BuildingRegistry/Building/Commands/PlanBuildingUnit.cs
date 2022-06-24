namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class PlanBuildingUnit : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("f7de0c52-a994-43ea-8a94-7114e81b8ceb");
        
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public BuildingUnitPositionGeometryMethod PositionGeometryMethod { get; }
        public ExtendedWkbGeometry? Position { get; }
        public BuildingUnitFunction Function { get; }
        public bool HasDeviation { get; }

        public Provenance Provenance { get; }

        public PlanBuildingUnit(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position,
            BuildingUnitFunction function,
            bool hasDeviation,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            PositionGeometryMethod = positionGeometryMethod;
            Position = position;
            Function = function;
            HasDeviation = hasDeviation;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"PlanBuildingUnit-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;
            yield return PositionGeometryMethod.ToString();
            yield return Position ?? string.Empty;
            yield return Function.ToString();
            yield return HasDeviation.ToString();

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
