namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Responses;

    public record GetReferencesRequest(LegacyContext Context, int PersistentLocalId, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingReferencesResponse>;
}
