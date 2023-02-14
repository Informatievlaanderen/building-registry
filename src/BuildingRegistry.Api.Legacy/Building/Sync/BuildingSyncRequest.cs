namespace BuildingRegistry.Api.Legacy.Building.Sync
{
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record SyncRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<SyncResponse>;
}
