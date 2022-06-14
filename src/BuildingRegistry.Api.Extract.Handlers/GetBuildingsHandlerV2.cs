namespace BuildingRegistry.Api.Extract.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Extracts;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public class GetBuildingsHandlerV2 : IRequestHandler<GetBuildingsRequest, FileResult>
    {
        public Task<FileResult> Handle(GetBuildingsRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), request.Context)
            {
                BuildingRegistryExtractV2Builder.CreateBuildingFiles(request.Context),
                BuildingUnitRegistryExtractV2Builder.CreateBuildingUnitFiles(request.Context)
            }
            .CreateFileCallbackResult(cancellationToken));
    }
}
