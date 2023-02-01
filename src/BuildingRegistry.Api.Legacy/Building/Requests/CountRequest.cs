namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;

    public record CountRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>;
}
