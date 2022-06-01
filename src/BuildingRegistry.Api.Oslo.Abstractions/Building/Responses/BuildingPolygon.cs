namespace BuildingRegistry.Api.Oslo.Abstractions.Building.Responses
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Newtonsoft.Json;

    /// <summary>
    /// De geometrie van het object in gml-formaat.
    /// </summary>
    [DataContract(Name = "GebouwPolygoon", Namespace = "")]
    public class BuildingPolygon
    {
        /// <summary>
        /// De geometrie.
        /// </summary>
        [JsonProperty("geometrie")]
        [XmlIgnore]
        public GmlJsonPolygon Geometry { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [DataMember(Name = "GeometrieMethode", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GeometrieMethode GeometryMethod { get; set; }

        public BuildingPolygon(GmlJsonPolygon geometry, GeometrieMethode geometryMethod)
        {
            Geometry = geometry;
            GeometryMethod = geometryMethod;
        }
    }
}
