namespace BuildingRegistry.Api.Legacy.Building.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record CountRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>;
}
