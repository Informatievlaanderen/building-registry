namespace BuildingRegistry.Api.Oslo.BuildingUnit.Detail
{
    using BuildingRegistry.Api.Oslo.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.Extensions.Options;

    public record GetRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        int PersistentLocalId) : IRequest<BuildingUnitOsloResponseWithEtag>;
}
