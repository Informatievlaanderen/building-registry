namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Detail
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// De geometrie van het object in gml-formaat.
    /// </summary>
    public class BuildingUnitPositionV3
    {
        /// <summary>
        /// De geometrie.
        /// </summary>
        [JsonProperty(PropertyName = "geometrie", Order = 0)]
        public List<PointGeometrie> Geometry { get; set; }

        /// <summary>
        /// De geometriemethode van de gebouweenheidpositie.
        /// </summary>
        [JsonProperty(PropertyName = "methode", Order = 1, Required = Required.DisallowNull)]
        public GebouweenheidPositieGeometrieMethodeV3 GeometryMethod { get; set; }

        public BuildingUnitPositionV3(List<PointGeometrie> geometries, PositieGeometrieMethode geometryMethod)
        {
            Geometry = geometries;
            GeometryMethod = new GebouweenheidPositieGeometrieMethodeV3(geometryMethod);
        }

        public BuildingUnitPositionV3(List<string> gmlGeometries, PositieGeometrieMethode geometryMethod)
        {
            Geometry = gmlGeometries.ConvertAll(gml => new PointGeometrie(gml));
            GeometryMethod = new GebouweenheidPositieGeometrieMethodeV3(geometryMethod);
        }
    }

    /// <summary>
    /// De gebruikte methode om de positie te bepalen.
    /// </summary>
    public class GebouweenheidPositieGeometrieMethodeV3
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
        public PositieGeometrieMethode Label { get; set; }

        public GebouweenheidPositieGeometrieMethodeV3(PositieGeometrieMethode positieGeometrieMethode)
        {
            Label = positieGeometrieMethode;
            Id = OsloNamespaces.AdresGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(positieGeometrieMethode.ToString(), false));
        }
    }
}
