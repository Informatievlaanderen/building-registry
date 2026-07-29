namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Detail
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Newtonsoft.Json;

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
        public PositieGeometrieMethode GeometryMethod { get; set; }

        public BuildingUnitPositionV3(List<PointGeometrie> geometries, PositieGeometrieMethode geometryMethod)
        {
            Geometry = geometries;
            GeometryMethod = geometryMethod;
        }

        public BuildingUnitPositionV3(List<string> gmlGeometries, PositieGeometrieMethode geometryMethod)
        {
            Geometry = gmlGeometries.ConvertAll(gml => new PointGeometrie(gml));
            GeometryMethod = geometryMethod;
        }
    }
}
