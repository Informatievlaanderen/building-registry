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
        public const string StatusName = "status";
        public const string GeometryMethod = "geometrie.methode";
        public const string Geometry = "geometrie.geometrie";
    }
}
