namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Amazon;
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
            // TODO: get from environemtn vars
            const string accessKey = "";
            const string secretKey = "";
            const string sessionToken = "";
            var regionEndpoint = RegionEndpoint.EUWest1;

            var sqsOptions = new SqsOptions(accessKey, secretKey, sessionToken, regionEndpoint);
            string queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(CrabImport)}";
            var queueUrl = await SqsQueue.CreateQueue(sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }

        public static dynamic? CreateSafeCommandId(dynamic? dyn) => IsValidGuid(dyn)
            ? dyn!.CreateCommandId()
            : ((dynamic?)Guid.Empty)!.CreateCommandId();

        public static bool IsValidGuid([NotNullWhen(true)] dynamic? dyn) => dyn is not null && Guid.TryParse(dyn, out Guid _);
    }
}
