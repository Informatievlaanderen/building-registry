namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class MergeBuildings : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("6F6D3E1D-5F1B-4B0B-9F6C-9F4F5F6E6D6C");

        public BuildingPersistentLocalId NewBuildingPersistentLocalId { get; set; }

        public ExtendedWkbGeometry NewExtendedWkbGeometry { get; set; }

        public IEnumerable<BuildingPersistentLocalId> BuildingPersistentLocalIdsToMerge { get; set; }

        public Provenance Provenance { get; }

        public MergeBuildings(
            BuildingPersistentLocalId newBuildingPersistentLocalId,
            ExtendedWkbGeometry newExtendedWkbGeometry,
            IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIdsToMerge,
            Provenance provenance)
        {
            NewBuildingPersistentLocalId = newBuildingPersistentLocalId;
            NewExtendedWkbGeometry = newExtendedWkbGeometry;
            BuildingPersistentLocalIdsToMerge = buildingPersistentLocalIdsToMerge;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MergeBuildings-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return NewBuildingPersistentLocalId;
            yield return NewExtendedWkbGeometry.ToString();

            foreach (var field in BuildingPersistentLocalIdsToMerge)
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
