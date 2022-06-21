namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs.Lambda
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class SqsPostHandler : IRequestHandler<SqsPostRequest, Unit>
    {
        private readonly ILogger<SqsPostHandler> _logger;

        public SqsPostHandler(ILogger<SqsPostHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPostRequest request, CancellationToken cancellationToken)
        {
            return Unit.Value;
            //var tags = new ConcurrentBag<long?>();

            //try
            //{
            //    var commandsPerCommandId = request.RegisterCrabImportList
            //        .SelectMany(x => x)
            //        .Select(RegisterCrabImportRequestMapping.Map)
            //        .Distinct(new LambdaEqualityComparer<dynamic>(x => (string)x.CreateCommandId().ToString()))
            //        .ToDictionary(x => (Guid?)x.CreateCommandId(), x => x);

            //    var tag = await request.Bus.IdempotentCommandHandlerDispatchBatch(
            //        commandsPerCommandId,
            //        request.Metadata,
            //        cancellationToken);

            //    tags.Add(tag);
            //}
            //catch (IdempotentCommandHandlerModule.InvalidCommandException)
            //{
            //    throw new ApiException("Ongeldig verzoek id", StatusCodes.Status400BadRequest);
            //}
            //catch (Exception ex)
            //{
            //    var x = request.RegisterCrabImportList
            //        .SelectMany(x => x)
            //        .Select(RegisterCrabImportRequestMapping.Map);
            //    _logger.LogError(ex, "Import error for id {TerrainObjectId}", new List<dynamic> { x.First().TerrainObjectId });
            //    throw;
            //}

            //return Unit.Value;
        }
    }
}
