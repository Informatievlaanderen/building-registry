namespace BuildingRegistry.Api.Oslo.Building.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Query;

    public record BuildingCountRequest(
        FilteringHeader<BuildingFilter> FilteringHeader,
        SortingHeader SortingHeader)
        : IRequest<TotaalAantalResponse>;
}
