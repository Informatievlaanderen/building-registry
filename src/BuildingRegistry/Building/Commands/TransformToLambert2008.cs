namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    /// <summary>
    /// Transforms the geometry and every building unit position the building holds to Lambert 2008
    /// (EPSG 3812), see ADR 0006. It takes a <see cref="BuildingPersistentLocalId"/> and nothing else: the
    /// transformation has nothing to decide per building.
    /// </summary>
    public sealed class TransformToLambert2008 : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("8b2d5c7e-0f43-4a91-b6d8-3c1e9a742f05");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public TransformToLambert2008(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"TransformToLambert2008-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
