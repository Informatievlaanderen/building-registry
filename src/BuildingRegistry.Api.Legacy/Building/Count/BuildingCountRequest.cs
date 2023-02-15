namespace BuildingRegistry.Api.Legacy.Building.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Query;

    public record CountRequest(
        LegacyContext Context,
        FilteringHeader<BuildingFilter> BuildingFilterHeader,
        SortingHeader SortingHeader
        ) : IRequest<TotaalAantalResponse>;
}
