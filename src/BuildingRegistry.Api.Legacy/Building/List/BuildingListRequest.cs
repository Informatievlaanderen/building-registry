namespace BuildingRegistry.Api.Legacy.Building.List
{
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public record ListRequest(LegacyContext Context, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingListResponse>;
}
