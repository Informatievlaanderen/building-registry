namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using Infrastructure.Grb;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Responses;

    public record GetRequest(LegacyContext Context, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions, IGrbBuildingParcel GrbBuildingParcel, int PersistentLocalId) : IRequest<BuildingResponse>;
}
