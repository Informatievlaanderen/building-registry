namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using NetTopologySuite.Geometries;
    using Requests;
    using Validation;

    public class ChangeBuildingOutlineRequestValidator : AbstractValidator<ChangeBuildingOutlineRequest>
    {
        public ChangeBuildingOutlineRequestValidator()
        {
            Polygon? polygon = null;
            RuleFor(x => x.GeometriePolygoon)
                .Must(gml => GmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader(), null, out polygon))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);

            When(_ => polygon is not null, () =>
            {
                RuleFor(x => x.GeometriePolygoon)
                    .Must(x => polygon!.Area > 1)
                    .WithErrorCode(ValidationErrors.Common.BuildingTooSmallGeometry.Code)
                    .WithMessage(ValidationErrors.Common.BuildingTooSmallGeometry.Message);
            });
        }}
}
