namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using FluentValidation;
    using Requests;

    public class PlaceBuildingUnderConstructionRequestValidator: AbstractValidator<PlaceBuildingUnderConstructionRequest>
    {
        public PlaceBuildingUnderConstructionRequestValidator()
        {
            // todo: return error code & message
            RuleFor(x => x.PersistentLocalId)
                .NotEmpty();
        }
    }
}
