namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using MediatR;

    public record GetReferencesRequest(int PersistentLocalId) : IRequest<BuildingDetailReferencesResponse>;
}
