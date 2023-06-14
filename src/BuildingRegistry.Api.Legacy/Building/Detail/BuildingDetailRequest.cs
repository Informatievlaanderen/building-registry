namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using MediatR;

    public record GetRequest(int PersistentLocalId) : IRequest<BuildingDetailResponseWithEtag>;
}
