namespace BuildingRegistry.Api.Oslo.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Responses;

    public record ListRequest(HttpRequest HttpRequest, HttpResponse HttpResponse, LegacyContext Context, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingListOsloResponse>;
}
