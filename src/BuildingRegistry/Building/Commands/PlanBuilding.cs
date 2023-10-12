namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class PlanBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("93e158d7-ac5d-4fa9-9bcc-841ca682281c");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public Provenance Provenance { get; }

        public PlanBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry geometry,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Geometry = geometry;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"PlanBuilding-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return Geometry.ToString();

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
