namespace BuildingRegistry.ValueObjects
{
    using NetTopologySuite.IO;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BuildingGeometry : ValueObject<BuildingGeometry>
    {
        private readonly WKBReader _wkbReader = new WKBReader { HandleSRID = true };

        public ExtendedWkbGeometry Geometry { get; }
        public BuildingGeometryMethod Method { get; }

        public BuildingGeometry(
            [JsonProperty("geometry")] ExtendedWkbGeometry geometry,
            [JsonProperty("geometryMethod")] BuildingGeometryMethod geometryMethod)
        {
            Geometry = geometry;
            Method = geometryMethod;
        }

        public bool Contains(ExtendedWkbGeometry geometry)
        {
            var buildingUnitGeometry = _wkbReader.Read(geometry);
            return _wkbReader.Read(Geometry).Contains(buildingUnitGeometry);
        }

        public ExtendedWkbGeometry Center =>
            ExtendedWkbGeometry.CreateEWkb(_wkbReader.Read(Geometry).Centroid.AsBinary());

        protected override IEnumerable<object> Reflect()
        {
            yield return Geometry;
            yield return Method;
        }
    }
}
