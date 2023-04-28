namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Datastructures;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class MeasureBuildingRequest
    {
        [DataMember(Name = "GeometriePolygoon", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }

        public GrbData GrbData { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public MeasureBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new MeasureBuilding(
                buildingPersistentLocalId,
                GeometriePolygoon.ToExtendedWkbGeometry(),
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
