namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Requests;

    public class ChangeBuildingOutlineRequestValidator : AbstractValidator<ChangeBuildingOutlineRequest>
    {
        public ChangeBuildingOutlineRequestValidator()
        {
            RuleFor(x => x.GeometriePolygoon)
                .Must(gml => GmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .WithErrorCode(ValidationErrorCodes.Building.InvalidPolygonGeometry)
                .WithMessage(ValidationErrorMessages.Building.InvalidPolygonGeometry);
        }}
}
