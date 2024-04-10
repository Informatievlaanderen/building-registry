namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Requests;
    using Validation;

    public class MoveBuildingUnitRequestValidator : AbstractValidator<MoveBuildingUnitRequest>
    {
        public MoveBuildingUnitRequestValidator(BuildingExistsValidator buildingExistsValidator)
        {
            RuleFor(x => x.DoelgebouwId)
                .Must(puri => OsloPuriValidator.TryParseIdentifier(puri, out var id) && int.TryParse(id, out _))
                .DependentRules(() =>
                {
                    RuleFor(x => x.DoelgebouwId)
                        .MustAsync(async (puri, cancellationToken) =>
                            OsloPuriValidator.TryParseIdentifier(puri, out var id)
                            && int.TryParse(id, out var buildingPersistentLocalId)
                            && await buildingExistsValidator.Exists(new BuildingPersistentLocalId(buildingPersistentLocalId), cancellationToken))
                        .WithErrorCode(ValidationErrors.MoveBuildingUnit.BuildingNotFound.Code)
                        .WithMessage((_, puri) => ValidationErrors.MoveBuildingUnit.BuildingNotFound.MessageWithPuri(puri));
                })
                .WithErrorCode(ValidationErrors.Common.BuildingIdInvalid.Code)
                .WithMessage(ValidationErrors.Common.BuildingIdInvalid.Message);
        }
    }
}
