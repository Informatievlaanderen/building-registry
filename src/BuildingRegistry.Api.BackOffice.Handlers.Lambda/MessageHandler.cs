namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using MediatR;
    using Requests.Building;
    using Requests.BuildingUnit;
    using Sqs.Requests.Building;
    using Sqs.Requests.BuildingUnit;

    public class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest sqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(sqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            switch (sqsRequest)
            {
                case NotRealizeBuildingSqsRequest request:
                    await mediator.Send(
                        new NotRealizeBuildingLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CorrectBuildingNotRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingNotRealizationLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case PlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(
                        new PlaceBuildingUnderConstructionLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CorrectPlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(
                        new CorrectPlaceBuildingUnderConstructionLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case PlanBuildingSqsRequest request:
                    await mediator.Send(
                        new PlanBuildingLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case RealizeBuildingSqsRequest request:
                    await mediator.Send(
                        new RealizeBuildingLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CorrectBuildingUnitRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingUnitRealizationLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CorrectBuildingRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingRealizationLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case NotRealizeBuildingUnitSqsRequest request:
                    await mediator.Send(
                        new NotRealizeBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CorrectBuildingUnitNotRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingUnitNotRealizationLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case PlanBuildingUnitSqsRequest request:
                    await mediator.Send(
                        new PlanBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case RealizeBuildingUnitSqsRequest request:
                    await mediator.Send(
                        new RealizeBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
