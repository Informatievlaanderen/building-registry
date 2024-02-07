// namespace BuildingRegistry.Building.Commands
// {
//     using System;
//     using System.Collections.Generic;
//     using Be.Vlaanderen.Basisregisters.Generators.Guid;
//     using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//     using Be.Vlaanderen.Basisregisters.Utilities;
//
//     public sealed class MarkBuildingAsMerged : IHasCommandProvenance
//     {
//         private static readonly Guid Namespace = new Guid("5b07611e-fc89-4077-bc24-2bf71fdbf5c6");
//
//         public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
//
//         public BuildingPersistentLocalId DestinationBuildingPersistentLocalId { get; }
//
//         public Provenance Provenance { get; }
//
//         public MarkBuildingAsMerged(BuildingPersistentLocalId buildingPersistentLocalId,
//             BuildingPersistentLocalId destinationBuildingPersistentLocalId,
//             Provenance provenance
//             )
//         {
//             BuildingPersistentLocalId = buildingPersistentLocalId;
//             DestinationBuildingPersistentLocalId = destinationBuildingPersistentLocalId;
//             Provenance = provenance;
//         }
//
//         public Guid CreateCommandId() => Deterministic.Create(Namespace, $"MarkBuildingAsMerged-{ToString()}");
//
//         public override string? ToString()
//             => ToStringBuilder.ToString(IdentityFields());
//
//         private IEnumerable<object> IdentityFields()
//         {
//             yield return BuildingPersistentLocalId;
//             yield return DestinationBuildingPersistentLocalId;
//
//             foreach (var field in Provenance.GetIdentityFields())
//             {
//                 yield return field;
//             }
//         }
//     }
// }
