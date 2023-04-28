namespace BuildingRegistry.Api.BackOffice.Abstractions.Building
{
    using System;
    using System.Runtime.Serialization;
    using BuildingRegistry.Building.Datastructures;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class GrbData
    {
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

        [DataMember(Name = "GeometriePolygoon", Order = 8)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }

        [DataMember(Name = "Overlap", Order = 9)]
        [JsonProperty(Required = Required.AllowNull)]
        public decimal? Overlap { get; set; }

        public BuildingGrbData ToBuildingGrbData()
        {
            return new BuildingGrbData(
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
                Overlap);
        }
    }
}
