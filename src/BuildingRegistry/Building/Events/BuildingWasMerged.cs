namespace BuildingRegistry.Building.Events;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Newtonsoft.Json;

[EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
[EventName(EventName)]
[EventDescription("Het gebouw werd samengevoegd.")]
public sealed class BuildingWasMerged : IBuildingEvent
{
    public const string EventName = "BuildingWasMerged"; // BE CAREFUL CHANGING THIS!!

    public int BuildingPersistentLocalId { get; }
    public int NewBuildingPersistentLocalId { get; }

    public ProvenanceData Provenance { get; private set; }

    public BuildingWasMerged(
        BuildingPersistentLocalId buildingPersistentLocalId,
        BuildingPersistentLocalId newBuildingPersistentLocalId)
    {
        BuildingPersistentLocalId = buildingPersistentLocalId;
        NewBuildingPersistentLocalId = newBuildingPersistentLocalId;
    }

    [JsonConstructor]
    private BuildingWasMerged(
        int buildingPersistentLocalId,
        int newBuildingPersistentLocalId,
        ProvenanceData provenance)
        : this(
            new BuildingPersistentLocalId(buildingPersistentLocalId),
            new BuildingPersistentLocalId(newBuildingPersistentLocalId))
    {
        ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
    }

    void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

    public IEnumerable<string> GetHashFields()
    {
        var fields = Provenance.GetHashFields().ToList();
        fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
        fields.Add(NewBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));

        return fields;
    }

    public string GetHash() => this.ToEventHash(EventName);
}