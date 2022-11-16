namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("De functie van de gebouweenheid werd gewijzigd.")]
    public sealed class BuildingUnitFunctionWasChanged : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitFunctionWasChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Functie van de gebouweenheid.")]
        public string Function { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitFunctionWasChanged(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitFunction function)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Function = function;
        }

        [JsonConstructor]
        private BuildingUnitFunctionWasChanged(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string function,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingUnitFunction.Parse(function))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(Function);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
