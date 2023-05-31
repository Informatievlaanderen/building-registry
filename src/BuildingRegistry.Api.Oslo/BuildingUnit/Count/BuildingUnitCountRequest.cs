namespace BuildingRegistry.Api.Oslo.BuildingUnit.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record CountRequest(LegacyContext Context, SyndicationContext SyndicationContext, HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>;
}