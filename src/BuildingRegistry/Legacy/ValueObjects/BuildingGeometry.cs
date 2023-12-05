namespace BuildingRegistry.Legacy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class BuildingGeometry : ValueObject<BuildingGeometry>
    {
        private readonly WKBReader _wkbReader = new WKBReader { HandleSRID = true };

        public ExtendedWkbGeometry Geometry { get; }
        public BuildingGeometryMethod Method { get; }

        public BuildingGeometry(
            [JsonProperty("geometry")] ExtendedWkbGeometry geometry,
            [JsonProperty("geometryMethod")] BuildingGeometryMethod geometryMethod)
        {
            Geometry = CleanUpGeometryCollection(geometry);
            Method = geometryMethod;
        }

        public bool Contains(ExtendedWkbGeometry geometry)
        {
            var buildingUnitGeometry = _wkbReader.Read(geometry);
            return _wkbReader.Read(Geometry).Contains(buildingUnitGeometry);
        }

        private ExtendedWkbGeometry CleanUpGeometryCollection(ExtendedWkbGeometry geometry)
        {
            var buildingGeometry = _wkbReader.Read(geometry);
            if (buildingGeometry is GeometryCollection gc && buildingGeometry.OgcGeometryType != OgcGeometryType.MultiPolygon)
            {
                var polygon = gc.Single(x => x is Polygon);
                return new ExtendedWkbGeometry(polygon.AsBinary());
            }

            return geometry;
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
