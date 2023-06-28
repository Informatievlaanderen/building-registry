namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Requests;
    using Validation;

    public class MergeBuildingRequestValidator: AbstractValidator<MergeBuildingRequest>
    {
        public MergeBuildingRequestValidator()
        {
            RuleFor(x => x.GeometriePolygoon)
                .Must(gml => GmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);

            RuleFor(x => x.SamenvoegenGebouwen)
                .Must(y => y.Any() && y.Count >= 2)
                .WithErrorCode(ValidationErrors.MergeBuildings.TooFewBuildings.Code)
                .WithMessage(ValidationErrors.MergeBuildings.TooFewBuildings.Message);

            RuleFor(x => x.SamenvoegenGebouwen)
                .Must(y => y.Any() && y.Count <= 20)
                .WithErrorCode(ValidationErrors.MergeBuildings.TooManyBuildings.Code)
                .WithMessage(ValidationErrors.MergeBuildings.TooManyBuildings.Message);
        }
    }
}
