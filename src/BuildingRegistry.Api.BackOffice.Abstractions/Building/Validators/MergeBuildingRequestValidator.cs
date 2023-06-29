namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Requests;
    using Validation;

    public class MergeBuildingRequestValidator : AbstractValidator<MergeBuildingRequest>
    {
        private readonly BuildingExistsValidator _buildingExistsValidator;

        public MergeBuildingRequestValidator(BuildingExistsValidator buildingExistsValidator)
        {
            _buildingExistsValidator = buildingExistsValidator;
        }

        public MergeBuildingRequestValidator()
        {
            RuleFor(x => x.SamenvoegenGebouwen)
                .Must(y => y.Any() && y.Count >= 2)
                .WithErrorCode(ValidationErrors.MergeBuildings.TooFewBuildings.Code)
                .WithMessage(ValidationErrors.MergeBuildings.TooFewBuildings.Message);

            RuleFor(x => x.SamenvoegenGebouwen)
                .Must(y => y.Any() && y.Count <= 20)
                .WithErrorCode(ValidationErrors.MergeBuildings.TooManyBuildings.Code)
                .WithMessage(ValidationErrors.MergeBuildings.TooManyBuildings.Message);

            RuleForEach(x => x.SamenvoegenGebouwen)
                .Must(puri => OsloPuriValidator.TryParseIdentifier(puri, out var id)
                    && int.TryParse(id, out _))
                .WithErrorCode(ValidationErrors.Common.BuildingIdInvalid.Code)
                .WithMessage(ValidationErrors.Common.BuildingIdInvalid.Message);

            RuleForEach(x => x.SamenvoegenGebouwen)
                .MustAsync(async (puri, cancellationToken) =>
                {
                    return OsloPuriValidator.TryParseIdentifier(puri, out var id)
                        && int.TryParse(id, out var buildingPersistentLocalId)
                        && !await _buildingExistsValidator.Exists(new BuildingPersistentLocalId(buildingPersistentLocalId), cancellationToken);
                })
                .WithErrorCode(ValidationErrors.MergeBuildings.BuildingNotFound.Code)
                .WithMessage((_, puri) => ValidationErrors.MergeBuildings.BuildingNotFound.MessageWithPuri(puri));

            RuleFor(x => x.GeometriePolygoon)
                .Must(gml => GmlPolygonValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingPolygonGeometry.Message);
        }
    }
}
