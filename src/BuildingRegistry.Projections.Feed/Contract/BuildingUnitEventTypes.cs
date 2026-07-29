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
        public const string StatusName = "status";
        public const string Function = "functie";
        public const string GeometryMethod = "positie.methode";
        public const string Position = "positie.geometrie";
        public const string AdresIds = "toegekendAdres";
        public const string GebouwId = "isDeelVan";
        public const string HasDeviation = "afwijkingVastgesteld";
    }
}
