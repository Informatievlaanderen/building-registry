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
                .Must(GrbGmlPolygonValidator.IsValid)
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);
        }}
}
