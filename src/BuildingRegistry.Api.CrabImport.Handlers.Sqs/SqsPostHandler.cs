namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Amazon;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Dasync.Collections;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public record SqsPostRequest(List<RegisterCrabImportRequest[]> RegisterCrabImportList, IDictionary<string, object> Metadata, IdempotentCommandHandlerModule Bus) : IRequest<Unit>;

    public record SqsPostResponse(ConcurrentBag<long?> Tags)
    {
        public ConcurrentBag<long?> Tags { get; set; } = Tags;
    }

    public class SqsPostHandler : IRequestHandler<SqsPostRequest, Unit>
    {
        private readonly ILogger<SqsPostHandler> _logger;

        public SqsPostHandler(ILogger<SqsPostHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPostRequest request, CancellationToken cancellationToken)
        {
            const string accessKey = "";
            const string secretKey = "";
            const string sessionToken = "";
            var regionEndpoint = RegionEndpoint.EUWest1;

            var commandsPerCommandId = request.RegisterCrabImportList//.ToList()
                .SelectMany(x => x)
                .Select(RegisterCrabImportRequestMapping.Map)
                .Distinct(new LambdaEqualityComparer<dynamic?>(x => x.CreateCommandId().ToString()))
                .ToDictionary(x => (Guid?)x!.CreateCommandId(), x => x);

            var sqsOptions = new SqsOptions(accessKey, secretKey, sessionToken, regionEndpoint);
            var queueUrl = await SqsQueue.CreateQueue(sqsOptions, nameof(SqsPostHandler), true, cancellationToken);

            foreach (var command in commandsPerCommandId)
            {
                _logger.LogDebug($"Copying command {command.Key} to queue");

                var groupId = command.Key.HasValue
                    ? command.Key.Value.ToString("D")
                    : "";
                await SqsProducer.Produce(sqsOptions, queueUrl, command.Value, groupId, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
