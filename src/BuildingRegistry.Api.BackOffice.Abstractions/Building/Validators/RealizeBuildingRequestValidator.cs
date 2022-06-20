namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using FluentValidation;
    using Requests;

    public class RealizeBuildingRequestValidator: AbstractValidator<RealizeBuildingRequest>
    {
        public RealizeBuildingRequestValidator()
        {
            // todo: return error code & message
            RuleFor(x => x.PersistentLocalId)
                .NotEmpty();
        }
    }
}
