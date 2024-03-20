namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using NetTopologySuite.IO.GML2;
    using NetTopologySuite.Operation.Valid;

    public static class GrbGmlPolygonValidator
    {
        public static bool IsValid(string? gml, GMLReader gmlReader) =>
            GmlPolygonValidator.IsValid(
                gml,
                gmlReader,
                x => new IsValidOp(x)
                {
                    IsSelfTouchingRingFormingHoleValid = true,
                    SelfTouchingRingFormingHoleValid = true
                });
    }
}
