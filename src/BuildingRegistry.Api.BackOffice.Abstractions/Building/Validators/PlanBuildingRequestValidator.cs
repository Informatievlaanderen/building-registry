namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using FluentValidation;
    using Requests;

    public class PlanBuildingRequestValidator: AbstractValidator<PlanBuildingRequest>
    {
        public PlanBuildingRequestValidator()
        {
            RuleFor(x => x.GeometriePolygoon)
                .Must(GmlPolygonValidator.IsValid)
                .WithErrorCode(ValidationErrorCodes.Building.InvalidPolygonGeometry)
                .WithMessage(ValidationErrorMessages.Building.InvalidPolygonGeometry);
        }
    }
}
