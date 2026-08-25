namespace BuildingRegistry.Api.Oslo.Building.V2.Sync
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;

    public static class BuildingUnitHelpers
    {
        /// <summary>
        /// The unit is part of the building's syndication object, and its <see cref="GmlPoint"/> carries no
        /// <c>srsName</c> either, so it follows the same <c>objectCrs</c> as the building it sits in.
        /// See ADR 0004.
        /// </summary>
        public static Point GetBuildingUnitPoint(byte[] point, int objectSrid)
        {
            var geometry = SyncGeometry.ToRequestedCrs(point, objectSrid);
            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToPointGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToPointGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
        }
    }
}
