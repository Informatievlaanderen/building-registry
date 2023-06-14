namespace BuildingRegistry.Api.Legacy.Building.Crab
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public record CrabGebouwenRequest(
        FilteringHeader<BuildingCrabMappingFilter> FilteringHeader,
        SortingHeader SortingHeader
        ) : IRequest<BuildingCrabMappingResponse?>;
}
