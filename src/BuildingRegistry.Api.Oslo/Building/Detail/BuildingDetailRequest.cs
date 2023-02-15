namespace BuildingRegistry.Api.Oslo.Building.Detail
{
    using BuildingRegistry.Api.Oslo.Infrastructure.Grb;
    using BuildingRegistry.Api.Oslo.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.Extensions.Options;

    public record BuildingDetailRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        IGrbBuildingParcel GrbBuildingParcel,
        int PersistentLocalId) : IRequest<BuildingOsloResponseWithEtag>;
}
