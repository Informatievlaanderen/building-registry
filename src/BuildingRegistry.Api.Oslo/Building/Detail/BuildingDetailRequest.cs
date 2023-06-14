namespace BuildingRegistry.Api.Oslo.Building.Detail
{
    using MediatR;

    public record BuildingDetailRequest(int PersistentLocalId) : IRequest<BuildingOsloResponseWithEtag>;
}
