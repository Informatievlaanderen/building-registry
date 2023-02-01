namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Responses;

    public record ListRequest(LegacyContext Context, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingListResponse>;
}
