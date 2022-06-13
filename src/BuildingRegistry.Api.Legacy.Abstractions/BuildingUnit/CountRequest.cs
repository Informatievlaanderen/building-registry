namespace BuildingRegistry.Api.Legacy.Abstractions.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;
    using Projections.Syndication;

    public record CountRequest(LegacyContext Context, SyndicationContext SyndicationContext, HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>;
}
