namespace BuildingRegistry.Api.Legacy.BuildingUnit.List
{
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public record BuildingUnitListRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        HttpRequest HttpRequest,
        HttpResponse HttpResponse)
            : IRequest<BuildingUnitListResponse>;
}
