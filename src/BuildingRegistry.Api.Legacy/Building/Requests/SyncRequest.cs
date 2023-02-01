namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public record SyncRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<SyncResponse>;
}
