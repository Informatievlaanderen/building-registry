namespace BuildingRegistry.Api.Extract.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Builders;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Requests;

    public class GetBuildingsHandlerV2 : IRequestHandler<GetBuildingsRequest, FileResult>
    {
        public Task<FileResult> Handle(GetBuildingsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), request.Context)
                {
                    BuildingRegistryExtractV2EsriBuilder.CreateBuildingFiles(request.Context),
                    BuildingUnitRegistryExtractV2Builder.CreateBuildingUnitFiles(request.Context)
                }
                .CreateFileCallbackResult(cancellationToken));
        }
    }
}
