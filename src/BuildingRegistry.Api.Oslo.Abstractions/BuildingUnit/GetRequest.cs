namespace BuildingRegistry.Api.Oslo.Abstractions.BuildingUnit
{
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Responses;

    public record GetRequest(LegacyContext Context, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions, int PersistentLocalId) : IRequest<BuildingUnitOsloResponse>;
}
