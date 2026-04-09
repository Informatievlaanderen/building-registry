namespace BuildingRegistry.Projections.Feed.Contract
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public static class BuildingUnitEventTypes
    {
        public const string CreateV1 = "basisregisters.buildingunit.create.v1";
        public const string UpdateV1 = "basisregisters.buildingunit.update.v1";
        public const string DeleteV1 = "basisregisters.buildingunit.delete.v1";
    }

    public static class BuildingUnitAttributeNames
    {
        public const string StatusName = "gebouweenheidStatus";
        public const string Function = "gebouweenheidFunctie";
        public const string GeometryMethod = "positieGeometrieMethode";
        public const string Position = "gebouweenheidPositie";
        public const string AdresIds = "adresIds";
        public const string GebouwId = "gebouwId";
        public const string HasDeviation = "afwijkingVastgesteld";
    }

    public sealed class BuildingUnitPositionCloudEventValue
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Point";

        [JsonProperty("projectie")]
        public string Projection { get; set; }

        [JsonProperty("gml")]
        public string Gml { get; set; }

        public BuildingUnitPositionCloudEventValue(string gml, string projection)
        {
            Gml = gml;
            Projection = projection;
        }
    }
}
