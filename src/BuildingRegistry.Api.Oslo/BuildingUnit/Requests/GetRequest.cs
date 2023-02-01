namespace BuildingRegistry.Api.Oslo.BuildingUnit.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Responses;

    public record GetRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        int PersistentLocalId) : IRequest<BuildingUnitOsloResponseWithEtag>;
}
