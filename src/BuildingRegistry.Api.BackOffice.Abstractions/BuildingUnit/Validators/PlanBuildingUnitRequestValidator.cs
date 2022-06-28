namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using FluentValidation;
    using Requests;

    public class PlanBuildingUnitRequestValidator: AbstractValidator<PlanBuildingUnitRequest>
    {
        public PlanBuildingUnitRequestValidator()
        {
           // 
        }
    }
}
