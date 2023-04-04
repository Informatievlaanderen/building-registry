namespace BuildingRegistry.Api.Extract.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Builders;
    using MediatR;
    using Projections.Extract;
    using Requests;

    public class GetBuildingUnitAddressLinksHandler : IRequestHandler<GetBuildingUnitAddressLinksRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetBuildingUnitAddressLinksHandler(ExtractContext context)
        {
            _context = context;
        }

        public Task<IsolationExtractArchive> Handle(GetBuildingUnitAddressLinksRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.AddressLinkExtractFileName, _context)
            {
                BuildingRegistryAddressLinkExtractBuilder.CreateBuildingUnitFiles(_context)
            });
        }
    }
}
