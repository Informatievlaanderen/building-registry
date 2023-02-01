namespace BuildingRegistry.Api.Legacy.BuildingUnit.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Responses;

    public record ListRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        HttpRequest HttpRequest,
        HttpResponse HttpResponse)
            : IRequest<BuildingUnitListResponse>;
}
