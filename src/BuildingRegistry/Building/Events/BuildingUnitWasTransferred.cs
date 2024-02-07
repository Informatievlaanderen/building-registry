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
//     [EventDescription("Gebouweenheid werd overgedragen naar het gebouw.")]
//     public class BuildingUnitWasTransferred : IBuildingEvent, IHasBuildingUnitPersistentLocalId
//     {
//         public const string EventName = "BuildingUnitWasTransferred"; // BE CAREFUL CHANGING THIS!!
//
//         [EventPropertyDescription("Objectidentificator van het doelgebouw.")]
//         public int BuildingPersistentLocalId { get; }
//
//         [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
//         public int BuildingUnitPersistentLocalId { get; }
//
//         [EventPropertyDescription("Objectidentificator van het gebouw van waar de gebouweenheid werd overgedragen.")]
//         public int SourceBuildingPersistentLocalId { get; }
//
//         [EventPropertyDescription("Functie van de gebouweenheid. Mogelijkheden: Common of Unknown.")]
//         public string Function { get; }
//
//         [EventPropertyDescription("Status van de gebouweenheid. Mogelijkheden: Planned, Realized, Retired of NotRealized.")]
//         public string Status { get; }
//
//         [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de gebouweenheid.")]
//         public List<int> AddressPersistentLocalIds { get; }
//
//         [EventPropertyDescription("Geometriemethode van de gebouweenheid. Mogelijkheden: AppointedByAdministrator of DerivedFromObject.")]
//         public string GeometryMethod { get; }
//
//         [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
//         public string ExtendedWkbGeometry { get; }
//
//         [EventPropertyDescription("Geeft aan of de gebouweenheid een afwijking heeft.")]
//         public bool HasDeviation { get; }
//
//         [EventPropertyDescription("Metadata bij het event.")]
//         public ProvenanceData Provenance { get; private set; }
//
//         public BuildingUnitWasTransferred(
//             BuildingPersistentLocalId buildingPersistentLocalId,
//             BuildingUnit buildingUnit,
//             BuildingPersistentLocalId sourceBuildingPersistentLocalId,
//             BuildingUnitPosition buildingUnitPosition)
//         {
//             BuildingPersistentLocalId = buildingPersistentLocalId;
//             BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId;
//             Function = buildingUnit.Function;
//             Status = buildingUnit.Status;
//             AddressPersistentLocalIds = buildingUnit.AddressPersistentLocalIds.Select(x => (int)x).ToList();
//             GeometryMethod = buildingUnitPosition.GeometryMethod;
//             ExtendedWkbGeometry = buildingUnitPosition.Geometry.ToString();
//             HasDeviation = buildingUnit.HasDeviation;
//             SourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
//         }
//
//         [JsonConstructor]
//         private BuildingUnitWasTransferred(
//             int buildingPersistentLocalId,
//             int buildingUnitPersistentLocalId,
//             string function,
//             string status,
//             List<int> addressPersistentLocalIds,
//             string geometryMethod,
//             string extendedWkbGeometry,
//             bool hasDeviation,
//             int sourceBuildingPersistentLocalId,
//             ProvenanceData provenance)
//         {
//             BuildingPersistentLocalId = buildingPersistentLocalId;
//             BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
//             Function = function;
//             Status = status;
//             AddressPersistentLocalIds = addressPersistentLocalIds;
//             GeometryMethod = geometryMethod;
//             ExtendedWkbGeometry = extendedWkbGeometry;
//             HasDeviation = hasDeviation;
//             SourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
//
//             ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
//         }
//
//         void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
//
//         public IEnumerable<string> GetHashFields()
//         {
//             var fields = Provenance.GetHashFields().ToList();
//             fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
//             fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
//             fields.Add(SourceBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
//             fields.Add(Function);
//             fields.Add(Status);
//             fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
//             fields.Add(GeometryMethod);
//             fields.Add(ExtendedWkbGeometry);
//             fields.Add(HasDeviation.ToString());
//
//             return fields;
//         }
//
//         public string GetHash() => this.ToEventHash(EventName);
//     }
// }
