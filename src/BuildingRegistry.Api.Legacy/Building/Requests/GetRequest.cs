namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using Infrastructure.Grb;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Responses;

    public record GetRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        IGrbBuildingParcel GrbBuildingParcel,
        int PersistentLocalId) : IRequest<BuildingResponseWithEtag>;
}
