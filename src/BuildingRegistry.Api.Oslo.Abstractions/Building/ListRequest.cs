namespace BuildingRegistry.Api.Oslo.Abstractions.Building
{
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Responses;

    public record ListRequest(HttpRequest HttpRequest, HttpResponse HttpResponse, LegacyContext Context, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingListOsloResponse>;
}
