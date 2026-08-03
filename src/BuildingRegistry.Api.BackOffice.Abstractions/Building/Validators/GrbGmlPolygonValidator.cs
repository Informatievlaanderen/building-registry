namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using NetTopologySuite.Operation.Valid;

    public static class GrbGmlPolygonValidator
    {
        public static bool IsValid(string? gml) =>
            GmlPolygonValidator.IsValidPolygon(
                gml,
                x => new IsValidOp(x)
                {
                    IsSelfTouchingRingFormingHoleValid = true,
                    SelfTouchingRingFormingHoleValid = true
                });
    }
}
