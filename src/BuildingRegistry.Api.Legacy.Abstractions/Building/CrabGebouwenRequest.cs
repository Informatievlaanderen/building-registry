namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;
    using Responses;

    public record CrabGebouwenRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<BuildingCrabMappingResponse>;
}
