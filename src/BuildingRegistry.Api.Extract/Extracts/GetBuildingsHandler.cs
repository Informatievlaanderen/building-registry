namespace BuildingRegistry.Api.Extract.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Extracts;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public class GetBuildingsHandler : IRequestHandler<GetBuildingsRequest, FileResult>
    {
        public Task<FileResult> Handle(GetBuildingsRequest request, CancellationToken cancellationToken) => Task.FromResult(new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), request.Context)
            {
                BuildingRegistryExtractBuilder.CreateBuildingFiles(request.Context),
                BuildingUnitRegistryExtractBuilder.CreateBuildingUnitFiles(request.Context)
            }
            .CreateFileCallbackResult(cancellationToken));
    }
}
