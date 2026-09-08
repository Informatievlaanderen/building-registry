namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public sealed class BuildingGeometry : ValueObject<BuildingGeometry>
    {
        private readonly WKBReader _wkbReader = new WKBReader { HandleSRID = true };
        private readonly WKBWriter _wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };

        public ExtendedWkbGeometry Geometry { get; }
        public BuildingGeometryMethod Method { get; }

        public BuildingGeometry(
            ExtendedWkbGeometry geometry,
            BuildingGeometryMethod geometryMethod)
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
                return new ExtendedWkbGeometry(_wkbWriter.Write(polygon));
            }

            return geometry;
        }

        /// <summary>
        /// The position building units with geometry method DerivedFromObject take, in the reference system
        /// the building geometry itself is in.
        /// </summary>
        /// <remarks>
        /// The SRID is carried over explicitly rather than left to the reader's geometry factory, and
        /// geometries persisted before the event store wrote EWKB - which carry no SRID - are Lambert 72 by
        /// definition. Pinning Lambert 72 unconditionally, as this did through
        /// <see cref="ExtendedWkbGeometry.CreateEWkb"/>, throws the moment the event store holds Lambert
        /// 2008. See ADR 0006.
        /// </remarks>
        public ExtendedWkbGeometry Center
        {
            get
            {
                var geometry = _wkbReader.Read(Geometry);
                var srid = geometry.SRID > 0 ? geometry.SRID : ExtendedWkbGeometry.SridLambert72;

                return ExtendedWkbGeometry.Create(geometry.CentroidWithinArea().WithSrid(srid));
            }
        }

        public Geometry GetGeometry() => _wkbReader.Read(Geometry);
        public Geometry GetGeometry(ExtendedWkbGeometry geometry) => _wkbReader.Read(geometry);

        protected override IEnumerable<object> Reflect()
        {
            yield return Geometry;
            yield return Method;
        }
    }
}
