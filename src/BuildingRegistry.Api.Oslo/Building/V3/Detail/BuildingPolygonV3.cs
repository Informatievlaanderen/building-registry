namespace BuildingRegistry.Api.Oslo.Building.V3.Detail
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// De geometrie van het object in gml-formaat.
    /// </summary>
    public class BuildingPolygonV3
    {
        /// <summary>
        /// De geometrie.
        /// </summary>
        [JsonProperty(PropertyName = "geometrie", Order = 0)]
        public List<PolygonGeometrie> Geometry { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [JsonProperty(PropertyName = "methode", Order = 1, Required = Required.DisallowNull)]
        public GebouwPositieGeometrieMethodeV3 GeometryMethod { get; set; }

        public BuildingPolygonV3(List<PolygonGeometrie> geometries, GebouwGeometrieMethode geometryMethod)
        {
            Geometry = geometries;
            GeometryMethod = new GebouwPositieGeometrieMethodeV3(geometryMethod);
        }

        public BuildingPolygonV3(List<string> gmlGeometries, GebouwGeometrieMethode geometryMethod)
        {
            Geometry = gmlGeometries.ConvertAll(gml => new PolygonGeometrie(gml));
            GeometryMethod = new GebouwPositieGeometrieMethodeV3(geometryMethod);
        }
    }

    /// <summary>
    /// De gebruikte methode om de positie te bepalen.
    /// </summary>
    public class GebouwPositieGeometrieMethodeV3
    {
        private static readonly CamelCaseNamingStrategy NamingStrategy = new();

        /// <summary>
        /// Identificatie van de methode.
        /// </summary>
        [JsonProperty("@id", Required = Required.DisallowNull, Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// Linked data type van het object.
        /// </summary>
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 2)]
        public string Type => "Concept";

        /// <summary>
        /// De beschrijving van de methode.
        /// </summary>
        [JsonProperty("code", Required = Required.DisallowNull, Order = 3)]
        public GebouwGeometrieMethode Label { get; set; }

        public GebouwPositieGeometrieMethodeV3(GebouwGeometrieMethode gebouwGeometrieMethode)
        {
            Label = gebouwGeometrieMethode;
            Id = OsloNamespaces.GebouwGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(gebouwGeometrieMethode.ToString(), false));
        }
    }
}
