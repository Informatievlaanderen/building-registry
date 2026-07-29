namespace BuildingRegistry.Api.Oslo.BuildingUnit.V2.Detail
{
    using MediatR;

    public record GetRequest(int PersistentLocalId) : IRequest<BuildingUnitOsloResponseWithEtag>;
}
