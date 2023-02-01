namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public record CrabGebouwenRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<BuildingCrabMappingResponse?>;
}
