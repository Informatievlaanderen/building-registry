namespace BuildingRegistry.Api.Oslo.Building.V2.Detail
{
    using MediatR;

    public record BuildingDetailRequest(int PersistentLocalId) : IRequest<BuildingOsloResponseWithEtag>;
}
