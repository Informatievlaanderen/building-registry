namespace BuildingRegistry.Api.Legacy.BuildingUnit.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Query;

    public record CountRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        FilteringHeader<BuildingUnitFilter> FilteringHeader,
        SortingHeader SortingHeader
        ) : IRequest<TotaalAantalResponse>;
}
