namespace BuildingRegistry.Api.Legacy.Building.Crab
{
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record CrabGebouwenRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<BuildingCrabMappingResponse?>;
}
