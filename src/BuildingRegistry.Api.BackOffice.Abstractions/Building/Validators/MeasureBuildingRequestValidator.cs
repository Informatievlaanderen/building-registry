namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Requests;
    using Validation;

    public class MeasureBuildingRequestValidator : AbstractValidator<MeasureBuildingRequest>
    {
        public MeasureBuildingRequestValidator()
        {
            RuleFor(x => x.GrbData.GeometriePolygoon)
                .Must(gml => GmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);
        }}
}