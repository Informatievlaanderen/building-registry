namespace BuildingRegistry.Api.CrabImport.CrabImport
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CrabImport;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Requests;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using Dasync.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api.Messages;
    using ApiController = Infrastructure.ApiController;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabimport")]
    [ApiExplorerSettings(GroupName = "CRAB Import")]
    public class CrabImportController : ApiController
    {
        private const string CommandMessageTemplate = "Handled {CommandCount} commands in {Elapsed:0.0000} ms";
        private const string BatchMessageTemplate = "Handled {AggregateCount} aggregates ({CommandCount} commands) in {Elapsed:0.0000} ms";
        private static double GetElapsedMilliseconds(long start, long stop) => (stop - start) * 1000 / (double)Stopwatch.Frequency;

        /// <summary>
        /// Import een CRAB item.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="registerCrabImportList"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="logger"></param>
        /// <response code="202">Als het verzoek aanvaard is.</response>
        /// <response code="400">Als het verzoek ongeldige data bevat.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RegisterCrabImportRequest), typeof(RegisterCrabImportRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status202Accepted, typeof(RegisterCrabImportResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> Post(
            [FromServices] IdempotentCommandHandlerModule bus,
            [FromServices] ILogger<CrabImportController> logger,
            [FromBody] List<RegisterCrabImportRequest[]> registerCrabImportList,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tags = new ConcurrentBag<long?>();

            var start = Stopwatch.GetTimestamp();
            logger.LogDebug("Preparing to process commands for {AggregateCount} aggregates.", registerCrabImportList.Count);

            await registerCrabImportList.ParallelForEachAsync(async registerCrabImports =>
            {
                var startCommands = Stopwatch.GetTimestamp();

                try
                {
                    var commandsPerCommandId = registerCrabImports
                        .Select(RegisterCrabImportRequestMapping.Map)
                        .Distinct(new LambdaEqualityComparer<dynamic>(x => (string)x.CreateCommandId().ToString()))
                        .ToDictionary(x => (Guid?)x.CreateCommandId(), x => x);

                    var tag = await bus.IdempotentCommandHandlerDispatchBatch(
                        commandsPerCommandId,
                        GetMetadata(),
                        cancellationToken);

                    tags.Add(tag);
                }
                catch (IdempotentCommandHandlerModule.InvalidCommandException)
                {
                    throw new ApiException("Ongeldig verzoek id", StatusCodes.Status400BadRequest);
                }
                catch (Exception ex)
                {
                    var x = registerCrabImports.Select(RegisterCrabImportRequestMapping.Map).ToList();
                    Console.WriteLine($"Boom, {x.First()}");
                    logger.LogError(ex, "Import error for id {TerrainObjectId}", new List<dynamic> { x.First().TerrainObjectId });
                    throw;
                }

                var elapsedCommandsMs = GetElapsedMilliseconds(startCommands, Stopwatch.GetTimestamp());
                logger.LogDebug(CommandMessageTemplate, registerCrabImports.Length, elapsedCommandsMs);
            },
            cancellationToken: cancellationToken,
            maxDegreeOfParallelism: 0);

            var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
            logger.LogDebug(BatchMessageTemplate, registerCrabImportList.Count, registerCrabImportList.SelectMany(x => x).Count(), elapsedMs);
            return Accepted(tags.Any() ? tags.Max() : null);
        }

        [HttpGet("batch/{feed}")]
        public IActionResult GetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromRoute] string feed)
        {
            var status = context.LastBatchFor((ImportFeed)feed);
            return Ok(status);
        }

        [HttpPost("batch")]
        public IActionResult SetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromBody] BatchStatusUpdate batchStatus)
        {
            context.SetCurrent(batchStatus);
            context.SaveChanges();

            return Ok();
        }

        [HttpGet("status/{feed}")]
        public IActionResult GetStatus(
            [FromServices] CrabImportContext context,
            [FromRoute] string feed)
            => Ok(context.StatusFor((ImportFeed)feed));

        [HttpGet("status")]
        public IActionResult GetStatus(
            [FromServices] CrabImportContext context)
            => Ok(context.StatusForAllFeeds());
    }

    public class RegisterCrabImportResponseExamples : IExamplesProvider<object>
    {
        public object GetExamples() => new { };
    }
}
