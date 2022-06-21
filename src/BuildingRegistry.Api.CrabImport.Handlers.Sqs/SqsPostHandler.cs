namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqsPostHandler : IRequestHandler<SqsPostRequest, Unit>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqsPostHandler> _logger;

        public SqsPostHandler(IConfiguration configuration, ILogger<SqsPostHandler> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPostRequest request, CancellationToken cancellationToken)
        {
            var accessKey = _configuration.GetValue<string>("AWS_ACCESS_KEY_ID") ?? throw new InvalidOperationException("The AWS_ACCESS_KEY_ID configuration variable was not set.");
            var secretKey = _configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY") ?? throw new InvalidOperationException("The AWS_SECRET_ACCESS_KEY configuration variable was not set.");

            var sqsOptions = new SqsOptions(accessKey, secretKey);
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
