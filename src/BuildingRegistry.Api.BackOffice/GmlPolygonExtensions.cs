namespace BuildingRegistry.Api.BackOffice
{
    using System.Linq;
    using Abstractions.Building;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using NetTopologySuite.Geometries;

    public static class GmlPolygonExtensions
    {
        public static string ToCleanPolygon(this string gmlPolygon)
        {
            var gmlReader = GmlHelpers.CreateGmlReader();
            var polygon = gmlReader.Read(gmlPolygon);

            var cleanedPolygon = RemoveConsecutiveDuplicatePoints(polygon);
            return cleanedPolygon.ToGmlJsonPolygon()!.Gml;
        }

        private static Geometry RemoveConsecutiveDuplicatePoints(Geometry geometry)
        {
            if (geometry is Polygon polygon)
            {
                var shell = RemoveDuplicatesFromRing(polygon.ExteriorRing);
                var holes = polygon.InteriorRings
                    .Select(RemoveDuplicatesFromRing)
                    .ToArray();

                return geometry.Factory.CreatePolygon(shell, holes);
            }
            return geometry;
        }

        private static LinearRing RemoveDuplicatesFromRing(LineString ring)
        {
            var coords = ring.Coordinates
                .Where((c, i) => i == 0 || !c.Equals2D(ring.Coordinates[i - 1]))
                .ToArray();

            return ring.Factory.CreateLinearRing(coords);
        }
    }
}
