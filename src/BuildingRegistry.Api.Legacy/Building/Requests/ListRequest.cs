namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Responses;

    public record ListRequest(LegacyContext Context, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingListResponse>;
}
