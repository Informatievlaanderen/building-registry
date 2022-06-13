namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;

    public record SyncRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<SyncResponse>;
}
