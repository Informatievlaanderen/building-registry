namespace BuildingRegistry.Api.Legacy.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;

    public static class BuildingUnitHelpers
    {
        public static Point GetBuildingUnitPoint(byte[] point)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToPointGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToPointGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
        }
    }
}
