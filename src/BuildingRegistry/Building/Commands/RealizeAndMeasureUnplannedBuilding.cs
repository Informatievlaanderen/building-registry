namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Datastructures;

    public sealed class RealizeAndMeasureUnplannedBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("22e87692-b914-46a1-85f0-6c7e5a04fe1c");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public BuildingGrbData BuildingGrbData { get; }

        public Provenance Provenance { get; }

        public RealizeAndMeasureUnplannedBuilding(
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
            => Deterministic.Create(Namespace, $"RealizeAndMeasureUnplannedBuilding-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return Geometry;

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
