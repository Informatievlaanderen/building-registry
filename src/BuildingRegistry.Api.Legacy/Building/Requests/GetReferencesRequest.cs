namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Responses;

    public record GetReferencesRequest(LegacyContext Context, int PersistentLocalId, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingReferencesResponse>;
}
