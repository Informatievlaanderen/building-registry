namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.BuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.BuildingUnit.Extensions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;


    public class SqsRealizeBuildingUnitHandler : IRequestHandler<SqsRealizeBuildingUnitRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly ILogger<SqsPlanBuildingUnitHandler> _logger;

        public SqsRealizeBuildingUnitHandler(
            SqsOptions sqsOptions,
            BackOfficeContext backOfficeContext,
            ILogger<SqsPlanBuildingUnitHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _backOfficeContext = backOfficeContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            if (!request.PersistentLocalId.TryGetBuildingIdForBuildingUnit(_backOfficeContext, out var buildingPersistentLocalId))
            {
                throw new InvalidOperationException();
            }

            request.MessageGroupId = buildingPersistentLocalId.ToString();

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, request.MessageGroupId, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
