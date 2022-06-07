namespace BuildingRegistry.Api.CrabImport.Handlers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Dasync.Collections;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public record PostRequest(List<RegisterCrabImportRequest[]> RegisterCrabImportList, IDictionary<string, object> Metadata, IdempotentCommandHandlerModule Bus) : IRequest<PostResponse>;

    public record PostResponse(ConcurrentBag<long?> Tags)
    {
        public ConcurrentBag<long?> Tags { get; set; } = Tags;
    }

    public class PostHandler : IRequestHandler<PostRequest, PostResponse>
    {
        private readonly ILogger<PostHandler> _logger;

        public PostHandler(ILogger<PostHandler> logger)
        {
            _logger = logger;
        }

        public async Task<PostResponse> Handle(PostRequest request, CancellationToken cancellationToken)
        {
            var tags = new ConcurrentBag<long?>();

            await request.RegisterCrabImportList.ParallelForEachAsync(async registerCrabImports =>
            {
                try
                {
                    var commandsPerCommandId = registerCrabImports
                        .Select(RegisterCrabImportRequestMapping.Map)
                        .Distinct(new LambdaEqualityComparer<dynamic>(x => (string)x.CreateCommandId().ToString()))
                        .ToDictionary(x => (Guid?)x.CreateCommandId(), x => x);

                    var tag = await request.Bus.IdempotentCommandHandlerDispatchBatch(
                        commandsPerCommandId,
                        request.Metadata,
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
                    _logger.LogError(ex, "Import error for id {TerrainObjectId}", new List<dynamic> { x.First().TerrainObjectId });
                    throw;
                }
            }, cancellationToken: cancellationToken, maxDegreeOfParallelism: 0);

            return new PostResponse(tags);
        }
    }
}
