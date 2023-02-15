namespace BuildingRegistry.Api.Extract.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Builders;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Requests;

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
