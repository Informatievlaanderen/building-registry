namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.BuildingUnit
{
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
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
