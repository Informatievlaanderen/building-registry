namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Requests;
    using Validation;

    public class CorrectBuildingUnitPositionRequestValidator : AbstractValidator<CorrectBuildingUnitPositionRequest>
    {
        public CorrectBuildingUnitPositionRequestValidator()
        {
           RuleFor(x => x.Positie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithErrorCode(ValidationErrors.Common.BuildingUnitRequiredPosition.Code)
                .WithMessage(ValidationErrors.Common.BuildingUnitRequiredPosition.Message);

            RuleFor(x => x.Positie)
                .Must(GmlPointValidator.IsValidPoint)
                .When(x => !string.IsNullOrEmpty(x.Positie))
                .WithErrorCode(ValidationErrors.Common.InvalidBuildingUnitPosition.Code)
                .WithMessage(ValidationErrors.Common.InvalidBuildingUnitPosition.Message);
        }
    }
}
