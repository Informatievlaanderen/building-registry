namespace BuildingRegistry.Api.Oslo.Building.V3.Detail
{
    using MediatR;

    public record BuildingDetailRequest(int PersistentLocalId) : IRequest<BuildingOsloResponseWithEtag>;
}
