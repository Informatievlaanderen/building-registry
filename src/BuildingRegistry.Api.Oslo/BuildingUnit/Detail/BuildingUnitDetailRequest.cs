namespace BuildingRegistry.Api.Oslo.BuildingUnit.Detail
{
    using MediatR;

    public record GetRequest(int PersistentLocalId) : IRequest<BuildingUnitOsloResponseWithEtag>;
}
