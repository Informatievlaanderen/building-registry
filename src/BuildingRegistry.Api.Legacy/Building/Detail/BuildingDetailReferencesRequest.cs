namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.Extensions.Options;

    public record GetReferencesRequest(LegacyContext Context, int PersistentLocalId, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingDetailReferencesResponse>;
}
