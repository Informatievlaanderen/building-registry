namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Datastructures;

    public sealed class ChangeBuildingMeasurement : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("8abf65b1-1b88-404e-98d2-de6d76a41940");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public BuildingGrbData GrbData { get; }

        public Provenance Provenance { get; }

        public ChangeBuildingMeasurement(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry geometry,
            BuildingGrbData grbData,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Geometry = geometry;
            GrbData = grbData;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeBuildingMeasurement-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return Geometry.ToString();

            foreach (var field in GrbData.IdentityFields())
            {
                yield return field;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
