namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Datastructures;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class DemolishBuildingRequest
    {
        public int PersistentLocalId { get; set; }

        public GrbData GrbData { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public DemolishBuilding ToCommand(Provenance provenance)
        {
            return new DemolishBuilding(
                new BuildingPersistentLocalId(PersistentLocalId),
                new BuildingGrbData(
                    GrbData.Idn,
                    Instant.FromDateTimeOffset(DateTimeOffset.Parse(GrbData.VersionDate)),
                    GrbData.EndDate != null
                        ? Instant.FromDateTimeOffset(DateTimeOffset.Parse(GrbData.EndDate))
                        : null,
                    GrbData.IdnVersion,
                    GrbData.GrbObject,
                    GrbData.GrbObjectType,
                    GrbData.EventType,
                    GrbData.GeometriePolygoon,
                    GrbData.Overlap),
                provenance);
        }
    }
}
