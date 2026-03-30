namespace BuildingRegistry.Projections.Feed.Contract
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public static class BuildingEventTypes
    {
        public const string CreateV1 = "basisregisters.building.create.v1";
        public const string UpdateV1 = "basisregisters.building.update.v1";
        public const string DeleteV1 = "basisregisters.building.delete.v1";
    }

    public static class BuildingAttributeNames
    {
        public const string StatusName = "gebouwStatus";
        public const string GeometryMethod = "geometrieMethode";
        public const string Geometry = "gebouwGeometrie";
    }

    public sealed class BuildingGeometryCloudEventValue
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Polygon";

        [JsonProperty("projectie")]
        public string Projection { get; set; }

        [JsonProperty("gml")]
        public string Gml { get; set; } = string.Empty;

        public BuildingGeometryCloudEventValue(string gml, string projection)
        {
            Gml = gml;
            Projection = projection;
        }
    }
}
