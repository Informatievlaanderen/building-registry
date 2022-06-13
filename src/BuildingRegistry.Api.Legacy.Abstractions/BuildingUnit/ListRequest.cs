namespace BuildingRegistry.Api.Legacy.Abstractions.BuildingUnit
{
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Responses;

    public record ListRequest(LegacyContext Context, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingUnitListResponse>;
}
