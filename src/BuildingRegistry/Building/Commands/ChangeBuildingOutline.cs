namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ChangeBuildingOutline : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("05f90a48-dcd9-47de-8924-05b1bfb5769b");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public Provenance Provenance { get; }

        public ChangeBuildingOutline(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry geometry,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Geometry = geometry;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeBuildingOutline-{ToString()}");

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
