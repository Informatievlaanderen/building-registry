namespace BuildingRegistry.Api.Oslo.Building.List
{
    using BuildingRegistry.Api.Oslo.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public record BuildingListRequest(HttpRequest HttpRequest, HttpResponse HttpResponse, LegacyContext Context, IOptions<ResponseOptions> ResponseOptions) : IRequest<BuildingListOsloResponse>;
}
