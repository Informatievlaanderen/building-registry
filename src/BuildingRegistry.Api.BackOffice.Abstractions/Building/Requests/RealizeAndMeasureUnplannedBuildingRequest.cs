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
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "StelGebouwVast", Namespace = "")]
    public sealed class RealizeAndMeasureUnplannedBuildingRequest
    {
        [DataMember(Name = "GeometriePolygoon", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }

        [DataMember(Name = "Idn", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public long Idn { get; set; }

        [DataMember(Name = "VersionDate", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public string VersionDate { get; set; }

        [DataMember(Name = "EndDate", Order = 3)]
        [JsonProperty(Required = Required.AllowNull)]
        public string? EndDate { get; set; }

        [DataMember(Name = "IdnVersion", Order = 4)]
        [JsonProperty(Required = Required.Always)]
        public int IdnVersion { get; set; }

        [DataMember(Name = "GrbObject", Order = 5)]
        [JsonProperty(Required = Required.Always)]
        public string GrbObject { get; set; }

        [DataMember(Name = "GrbObjectType", Order = 6)]
        [JsonProperty(Required = Required.Always)]
        public string GrbObjectType { get; set; }

        [DataMember(Name = "EventType", Order = 7)]
        [JsonProperty(Required = Required.Always)]
        public string EventType { get; set; }

        [DataMember(Name = "Overlap", Order = 8)]
        [JsonProperty(Required = Required.AllowNull)]
        public decimal? Overlap { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public RealizeAndMeasureUnplannedBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new RealizeAndMeasureUnplannedBuilding(
                buildingPersistentLocalId,
                GeometriePolygoon.ToExtendedWkbGeometry(),
                new BuildingGrbData(
                    Idn,
                    Instant.FromDateTimeOffset(DateTimeOffset.Parse(VersionDate)),
                    EndDate != null
                        ? Instant.FromDateTimeOffset(DateTimeOffset.Parse(EndDate))
                        : null,
                    IdnVersion,
                    GrbObject,
                    GrbObjectType,
                    EventType,
                    GeometriePolygoon,
                    Overlap),
                provenance);
        }
    }

    public class RealizeAndMeasureUnplannedBuildingRequestExamples : IExamplesProvider<RealizeAndMeasureUnplannedBuildingRequest>
    {
        public RealizeAndMeasureUnplannedBuildingRequest GetExamples()
        {
            return new RealizeAndMeasureUnplannedBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
            };
        }
    }
}
