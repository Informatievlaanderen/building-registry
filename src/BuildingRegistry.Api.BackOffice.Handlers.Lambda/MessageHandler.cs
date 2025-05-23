namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Abstractions.Building.SqsRequests;
    using Abstractions.BuildingUnit.SqsRequests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using MediatR;
    using Requests.Building;
    using Requests.BuildingUnit;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Building;

    public class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken _)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest &&
                messageData is not NotifyOutlinedRealizedBuildingSqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(SqsRequest)} or {nameof(NotifyOutlinedRealizedBuildingSqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(5));
            var cancellationToken = cancellationTokenSource.Token;

            switch (messageData)
            {
                case PlanBuildingSqsRequest request:
                    await mediator.Send(new PlanBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RealizeBuildingSqsRequest request:
                    await mediator.Send(new RealizeBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingRealizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case NotRealizeBuildingSqsRequest request:
                    await mediator.Send(new NotRealizeBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingNotRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingNotRealizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case PlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(new PlaceBuildingUnderConstructionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectPlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(new CorrectPlaceBuildingUnderConstructionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case ChangeBuildingOutlineSqsRequest request:
                    await mediator.Send(new ChangeBuildingOutlineLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case PlanBuildingUnitSqsRequest request:
                    await mediator.Send(new PlanBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new RealizeBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitRealizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case NotRealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new NotRealizeBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitNotRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitNotRealizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RetireBuildingUnitSqsRequest request:
                    await mediator.Send(new RetireBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitRetirementSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitRetirementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitRemovalSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitRemovalLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitPositionSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitPositionLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RemoveBuildingUnitSqsRequest request:
                    await mediator.Send(new RemoveBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RemoveMeasuredBuildingSqsRequest request:
                    await mediator.Send(new RemoveMeasuredBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case DeregulateBuildingUnitSqsRequest request:
                    await mediator.Send(new DeregulateBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;
                case CorrectBuildingUnitDeregulationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingUnitDeregulationLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;
                case RegularizeBuildingUnitSqsRequest request:
                    await mediator.Send(new RegularizeBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingUnitRegularizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitRegularizationLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case AttachAddressToBuildingUnitSqsRequest request:
                    await mediator.Send(new AttachAddressToBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case DetachAddressFromBuildingUnitSqsRequest request:
                    await mediator.Send(new DetachAddressFromBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RemoveBuildingSqsRequest request:
                    await mediator.Send(new RemoveBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case RealizeAndMeasureUnplannedBuildingSqsRequest request:
                    await mediator.Send(new RealizeAndMeasureUnplannedBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case MeasureBuildingSqsRequest request:
                    await mediator.Send(new MeasureBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case CorrectBuildingMeasurementSqsRequest request:
                    await mediator.Send(new CorrectBuildingMeasurementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case DemolishBuildingSqsRequest request:
                    await mediator.Send(new DemolishBuildingLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case ChangeBuildingMeasurementSqsRequest request:
                    await mediator.Send(new ChangeBuildingMeasurementLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case MoveBuildingUnitSqsRequest request:
                    await mediator.Send(new MoveBuildingUnitLambdaRequest(messageMetadata.MessageGroupId!, request), cancellationToken);
                    break;

                case NotifyOutlinedRealizedBuildingSqsRequest request:
                    await mediator.Send(new NotifyOutlinedRealizedBuildingLambdaRequest(
                        request.BuildingPersistentLocalId,
                        request.Organisation,
                        request.DateTimeStatusChange,
                        new ExtendedWkbGeometry(request.ExtendedWkbGeometry)), cancellationToken);
                    break;

                case CreateBuildingOsloSnapshotsSqsRequest request:
                    await mediator.Send(
                        new CreateBuildingOsloSnapshotsLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CreateBuildingUnitOsloSnapshotsSqsRequest request:
                    await mediator.Send(
                        new CreateBuildingUnitOsloSnapshotsLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                case CreateBuildingSnapshotSqsRequest request:
                    await mediator.Send(
                        new CreateBuildingSnapshotLambdaRequest(messageMetadata.MessageGroupId!, request),
                        cancellationToken);
                    break;

                default:
                    throw new NotImplementedException(
                        $"{messageData.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
