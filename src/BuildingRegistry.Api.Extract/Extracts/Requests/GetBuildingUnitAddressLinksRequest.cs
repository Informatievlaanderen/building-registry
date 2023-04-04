namespace BuildingRegistry.Api.Extract.Extracts.Requests
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using MediatR;

    public struct GetBuildingUnitAddressLinksRequest : IRequest<IsolationExtractArchive>
    { }
}
