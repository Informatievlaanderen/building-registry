namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Datastructures;

    public sealed class CorrectBuildingMeasurement : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("f564e73f-c128-4510-82a8-734c04cf06fb");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public BuildingGrbData BuildingGrbData { get; }

        public Provenance Provenance { get; }

        public CorrectBuildingMeasurement(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry geometry,
            BuildingGrbData buildingGrbData,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Geometry = geometry;
            BuildingGrbData = buildingGrbData;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectBuildingMeasurement-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return Geometry.ToString();

            foreach (var field in BuildingGrbData.IdentityFields())
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
