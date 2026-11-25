namespace BuildingRegistry.Building
{
    using System;
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
        /// 2008. See ADR 0007.
        ///
        /// Rounded to centimetres through <see cref="ExtendedWkbGeometry.CreatePosition"/>, like every other
        /// building unit position. A centroid carries as many decimals as the outline it is computed from,
        /// so without this a derived position is the one kind the event store holds at full precision - and
        /// the Lambert 2008 transformation inherits it from here rather than rounding it the way it rounds a
        /// unit's own position. The building geometry itself stays unrounded; it is the position taken from
        /// it that is a position.
        ///
        /// The rounding is checked against the building: <c>CentroidWithinArea</c> decides inside or outside
        /// on the unrounded point, so a centroid a few millimetres inside the boundary of a concave building
        /// can round to a point just outside it. The centre then falls back to the nearest centimetre grid
        /// point that is inside, around the centroid first and around the interior point next. Only a
        /// building too narrow to hold any grid point keeps the plain rounded centroid.
        /// </remarks>
        public ExtendedWkbGeometry Center
        {
            get
            {
                var geometry = _wkbReader.Read(Geometry);
                var srid = geometry.SRID > 0 ? geometry.SRID : ExtendedWkbGeometry.SridLambert72;
                var centroidWithinArea = geometry.CentroidWithinArea();

                return RoundedPositionWithin(geometry, centroidWithinArea, srid)
                       ?? RoundedPositionWithin(geometry, geometry.InteriorPoint, srid)
                       ?? ExtendedWkbGeometry.CreatePosition(centroidWithinArea.WithSrid(srid));
            }
        }

        private ExtendedWkbGeometry? RoundedPositionWithin(Geometry geometry, Geometry point, int srid)
        {
            var rounded = ExtendedWkbGeometry.CreatePosition(point.Copy().WithSrid(srid));
            if (Contains(rounded))
            {
                return rounded;
            }

            var scale = Math.Pow(10, GeometryReferenceSystem.PositionRoundingPrecision);
            var x = point.Coordinate.X;
            var y = point.Coordinate.Y;

            var gridPoints =
                from gridX in new[] { Math.Floor(x * scale) / scale, Math.Ceiling(x * scale) / scale }
                from gridY in new[] { Math.Floor(y * scale) / scale, Math.Ceiling(y * scale) / scale }
                orderby (gridX - x) * (gridX - x) + (gridY - y) * (gridY - y), gridX, gridY
                select geometry.Factory.CreatePoint(new Coordinate(gridX, gridY));

            return gridPoints
                .Select(gridPoint => ExtendedWkbGeometry.CreatePosition(gridPoint.WithSrid(srid)))
                .FirstOrDefault(Contains);
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
