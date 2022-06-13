namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Amazon;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
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
            const string accessKey = "";
            const string secretKey = "";
            const string sessionToken = "";
            var regionEndpoint = RegionEndpoint.EUWest1;

            var commandsPerCommandId = request.RegisterCrabImportList
                .SelectMany(x => x)
                .Select(RegisterCrabImportRequestMapping.Map)
                .Where(x => IsValidGuid(x))
                .Distinct(new LambdaEqualityComparer<dynamic?>(x => CreateSafeCommandId(x).ToString()))
                .ToDictionary(x => CreateSafeCommandId(x), x => x);

            var sqsOptions = new SqsOptions(accessKey, secretKey, sessionToken, regionEndpoint);
            var queueUrl = await SqsQueue.CreateQueue(sqsOptions, nameof(SqsPostHandler), true, cancellationToken);

            foreach (var command in commandsPerCommandId)
            {
                _logger.LogDebug($"Copying command {command.Key} to queue");

                var groupId = command.Key.HasValue
                    ? command.Key.Value.ToString("D")
                    : Guid.Empty.ToString("D");
                await SqsProducer.Produce(sqsOptions, queueUrl, command.Value, groupId, cancellationToken);
            }

            return Unit.Value;
        }

        public static dynamic? CreateSafeCommandId(dynamic? dyn) => IsValidGuid(dyn)
            ? dyn!.CreateCommandId()
            : ((dynamic?)Guid.Empty)!.CreateCommandId();

        public static bool IsValidGuid([NotNullWhen(true)] dynamic? dyn) => dyn is not null && Guid.TryParse(dyn, out Guid _);
    }
}
