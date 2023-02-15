namespace BuildingRegistry.Api.Legacy.Building.Crab
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Query;

    public record CrabGebouwenRequest(
        LegacyContext Context,
        FilteringHeader<BuildingCrabMappingFilter> FilteringHeader,
        SortingHeader SortingHeader
        ) : IRequest<BuildingCrabMappingResponse?>;
}
