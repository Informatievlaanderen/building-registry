namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public static class GrbGmlPolygonValidator
    {
        public static bool IsValid(
            string? gml,
            GMLReader gmlReader)
        {
            if (string.IsNullOrEmpty(gml) || !gml.Contains(GmlConstants.GmlVersionAttribute) || !gml.Contains(GmlConstants.SrsNameAttribute))
            {
                return false;
            }

            try
            {
                var geometry = gmlReader.Read(gml);

                return geometry is Polygon &&
                       new NetTopologySuite.Operation.Valid.IsValidOp(geometry)
                       {
                           IsSelfTouchingRingFormingHoleValid = true,
                           SelfTouchingRingFormingHoleValid = true
                       }.IsValid;
            }
            catch (XmlException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
