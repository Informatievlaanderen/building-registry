namespace BuildingRegistry.Api.BackOffice.BuildingUnit.Handlers.Sqs.Lambda.BuildingUnit
{
    using Abstractions.BuildingUnit.Requests;
    using MediatR;

    public class SqsPlanBuildingUnitHandler : IRequestHandler<SqsPlanBuildingUnitRequest, Unit>
    {
        public Task<Unit> Handle(SqsPlanBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            // do nothing
            return Task.FromResult(Unit.Value);
        }
    }
}
