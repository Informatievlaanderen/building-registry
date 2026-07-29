namespace BuildingRegistry.Api.Oslo.Building.V3.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using MediatR;
    using Query;

    public record BuildingCountRequest(
        FilteringHeader<BuildingFilter> FilteringHeader,
        SortingHeader SortingHeader)
        : IRequest<TotaalAantalResponse>;
}
