namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using BuildingRegistry.Api.Legacy.Infrastructure.Grb;
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.Extensions.Options;

    public record GetRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        IGrbBuildingParcel GrbBuildingParcel,
        int PersistentLocalId) : IRequest<BuildingDetailResponseWithEtag>;
}
