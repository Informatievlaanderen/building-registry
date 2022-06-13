namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using NetTopologySuite.Geometries;

    public static class GmlPolygonValidator
    {
        public const string GmlVersionAttribute = "xmlns:gml=\"http://www.opengis.net/gml/3.2\"";
        public const string SrsNameAttribute = "srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"";

        public static bool IsValid(string? gml)
        {
            if (string.IsNullOrEmpty(gml) || !gml.Contains(GmlVersionAttribute) || !gml.Contains(SrsNameAttribute))
            {
                return false;
            }

            var gmlReader = GmlHelpers.CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            if (geometry is null or not Polygon)
                return false;

            return geometry.IsValid;
        }
    }
}
