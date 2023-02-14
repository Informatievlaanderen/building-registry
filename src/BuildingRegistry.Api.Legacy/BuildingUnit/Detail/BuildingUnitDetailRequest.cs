namespace BuildingRegistry.Api.Legacy.BuildingUnit.Detail
{
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.Extensions.Options;

    public record GetBuildingUnitDetailRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        int PersistentLocalId) : IRequest<BuildingUnitResponseWithEtag>;
}
