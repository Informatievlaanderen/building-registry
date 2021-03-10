namespace BuildingRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Projections.Extract;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als gebouwregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> GetBuildings(
            [FromServices] ExtractContext context,
            CancellationToken cancellationToken = default)
        {
            return new IsolationExtractArchive(ExtractFileNames.GetBuildingZipName(), context)
                {
                    BuildingRegistryExtractBuilder.CreateBuildingFiles(context),
                    BuildingUnitRegistryExtractBuilder.CreateBuildingUnitFiles(context),
                }
               .CreateFileCallbackResult(cancellationToken);
        }
    }
}
