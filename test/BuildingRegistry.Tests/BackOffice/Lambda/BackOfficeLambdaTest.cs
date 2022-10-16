namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Moq;
    using Newtonsoft.Json;
    using TicketingService.Abstractions;
    using Xunit.Abstractions;

    public class BackOfficeLambdaTest : BuildingRegistryTest
    {
        protected BackOfficeLambdaTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        protected void PlanBuilding(BuildingPersistentLocalId buildingPersistentLocalId, ExtendedWkbGeometry? wkbGeometry = null)
        {
            DispatchArrangeCommand(new PlanBuilding(
                buildingPersistentLocalId,
                wkbGeometry ?? new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                Fixture.Create<Provenance>()));
        }

        protected void PlaceBuildingUnderConstruction(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            DispatchArrangeCommand(new PlaceBuildingUnderConstruction(
                buildingPersistentLocalId,
                Fixture.Create<Provenance>()));
        }

        protected void RealizeBuilding(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            DispatchArrangeCommand(new RealizeBuilding(
                buildingPersistentLocalId,
                Fixture.Create<Provenance>()));
        }

        protected void PlanBuildingUnit(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod? positionGeometryMethod = null,
            ExtendedWkbGeometry? position = null,
            BuildingUnitFunction? function = null,
            bool hasDeviation = false)
        {
            DispatchArrangeCommand(new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                positionGeometryMethod ?? BuildingUnitPositionGeometryMethod.DerivedFromObject,
                position,
                function ?? BuildingUnitFunction.Unknown,
                hasDeviation,
                Fixture.Create<Provenance>()));
        }

        protected Mock<ITicketing> MockTicketing(Action<ETagResponse> ticketingCompleteCallback)
        {
            var ticketing = new Mock<ITicketing>();
            ticketing
                .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
                .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
                {
                    var eTagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticketResult.ResultAsJson!)!;
                    ticketingCompleteCallback(eTagResponse);
                });

            return ticketing;
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>()
            where TException : Exception, new()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws<TException>();
            return idempotentCommandHandler;
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>(Func<TException> exceptionFactory)
            where TException : Exception
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws(exceptionFactory());
            return idempotentCommandHandler;
        }
    }
}
