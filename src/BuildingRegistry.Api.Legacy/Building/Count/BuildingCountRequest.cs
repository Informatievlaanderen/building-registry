namespace BuildingRegistry.Api.Legacy.Building.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Query;

    public record CountRequest(
        FilteringHeader<BuildingFilter> BuildingFilterHeader,
        SortingHeader SortingHeader
        ) : IRequest<TotaalAantalResponse>;
}
