namespace BuildingRegistry.Api.Extract.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Builders;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Requests;

    public class GetBuildingsHandlerV2 : IRequestHandler<GetBuildingsRequest, FileResult>
    {
        private readonly bool _useEsri;

        public GetBuildingsHandlerV2(IConfiguration configuration)
        {
            _useEsri = configuration.GetValue<bool>("UseEsri", false);
        }

        public Task<FileResult> Handle(GetBuildingsRequest request, CancellationToken cancellationToken)
        {
            if (_useEsri)
            {
                return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), request.Context)
                    {
                        BuildingRegistryExtractV2EsriBuilder.CreateBuildingFiles(request.Context),
                        BuildingUnitRegistryExtractV2Builder.CreateBuildingUnitFiles(request.Context)
                    }
                    .CreateFileCallbackResult(cancellationToken));
            }
            else
            {
                return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), request.Context)
                    {
                        BuildingRegistryExtractV2Builder.CreateBuildingFiles(request.Context),
                        BuildingUnitRegistryExtractV2Builder.CreateBuildingUnitFiles(request.Context)
                    }
                    .CreateFileCallbackResult(cancellationToken));
            }
        }
    }
}
