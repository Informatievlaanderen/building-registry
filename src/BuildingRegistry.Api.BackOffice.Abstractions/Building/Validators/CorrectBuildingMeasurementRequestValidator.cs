namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using FluentValidation;
    using Requests;
    using Validation;

    public class CorrectBuildingMeasurementRequestValidator : AbstractValidator<CorrectBuildingMeasurementRequest>
    {
        public CorrectBuildingMeasurementRequestValidator()
        {
            RuleFor(x => x.GrbData.GeometriePolygoon)
                .Must(gml => GrbGmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);
        }}
}
