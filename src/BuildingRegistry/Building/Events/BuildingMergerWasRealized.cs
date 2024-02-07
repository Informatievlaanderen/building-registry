// namespace BuildingRegistry.Building.Events
// {
//     using System.Collections.Generic;
//     using System.Globalization;
//     using System.Linq;
//     using Be.Vlaanderen.Basisregisters.EventHandling;
//     using Be.Vlaanderen.Basisregisters.GrAr.Common;
//     using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//     using Newtonsoft.Json;
//
//     [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
//     [EventName(EventName)]
//     [EventDescription("Gebouw samenvoeging werd gerealiseerd.")]
//     public sealed class BuildingMergerWasRealized : IBuildingEvent
//     {
//         public const string EventName = "BuildingMergerWasRealized"; // BE CAREFUL CHANGING THIS!!
//
//         [EventPropertyDescription("Objectidentificator van het gebouw.")]
//         public int BuildingPersistentLocalId { get; }
//
//         [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie (Hexadecimale notatie).")]
//         public string ExtendedWkbGeometry { get; }
//
//         [EventPropertyDescription("Objectidentificatoren van de samengevoegde gebouwen.")]
//         public IList<int> MergedBuildingPersistentLocalIds { get; }
//
//         [EventPropertyDescription("Metadata bij het event.")]
//         public ProvenanceData Provenance { get; private set; }
//
//         public BuildingMergerWasRealized(
//             BuildingPersistentLocalId buildingPersistentLocalId,
//             ExtendedWkbGeometry extendedWkbGeometry,
//             IEnumerable<BuildingPersistentLocalId> mergedBuildingPersistentLocalIds)
//         {
//             BuildingPersistentLocalId = buildingPersistentLocalId;
//             ExtendedWkbGeometry = extendedWkbGeometry;
//             MergedBuildingPersistentLocalIds = mergedBuildingPersistentLocalIds.Select(x => (int)x).ToList();
//         }
//
//         [JsonConstructor]
//         private BuildingMergerWasRealized(
//             int buildingPersistentLocalId,
//             string extendedWkbGeometry,
//             IList<int> mergedBuildingPersistentLocalIds,
//             ProvenanceData provenance)
//             : this(new BuildingPersistentLocalId(buildingPersistentLocalId),
//                 new ExtendedWkbGeometry(extendedWkbGeometry),
//                 mergedBuildingPersistentLocalIds.Select(x => new BuildingPersistentLocalId(x)))
//         {
//             ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
//         }
//
//         void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
//
//         public IEnumerable<string> GetHashFields()
//         {
//             var fields = Provenance.GetHashFields().ToList();
//             fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
//             fields.Add(ExtendedWkbGeometry);
//
//             fields.AddRange(MergedBuildingPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
//
//             return fields;
//         }
//
//         public string GetHash() => this.ToEventHash(EventName);
//     }
// }
