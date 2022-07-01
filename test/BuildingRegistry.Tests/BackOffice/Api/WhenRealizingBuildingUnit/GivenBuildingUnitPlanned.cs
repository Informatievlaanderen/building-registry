namespace BuildingRegistry.Tests.BackOffice.Api.WhenRealizingBuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Building;
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitPlanned : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingUnitController _controller;

        private readonly Mock<IBuildings> _mockBuildingsRepository;
        private readonly BackOfficeContext _backOfficeContext;

        public GivenBuildingUnitPlanned(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockBuildingsRepository = new Mock<IBuildings>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(_mockBuildingsRepository.Object, _backOfficeContext);
        }

        [Fact]
        public async Task WithoutIfMatchHeader_ThenAcceptedResponseIsExpected()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty));

            var request = new RealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.Realize(
                ResponseOptions,
                request,
                string.Empty,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithCorrectIfMatchHeader_ThenAcceptedResponseIsExpected()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var buildingStreamId = new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId));
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            Fixture.Register(() => buildingUnitPersistentLocalId);

            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(buildingUnitPersistentLocalId,
                buildingPersistentLocalId));
            _backOfficeContext.SaveChanges();

            var building = Building.Factory();
            var buildingUnitPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            var ifMatchHeader = buildingUnitPlanned.GetHash();

            building.Initialize(new[]
            {
                buildingUnitPlanned
            });

            _mockBuildingsRepository
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(building));

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty));

            var request = new RealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.Realize(
                ResponseOptions,
                request,
                new ETag(ETagType.Strong, ifMatchHeader).ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            Fixture.Register(() => buildingUnitPersistentLocalId);

            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(buildingUnitPersistentLocalId,
                buildingPersistentLocalId));
            _backOfficeContext.SaveChanges();

            var building = Building.Factory();
            var buildingUnitPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            var ifMatchHeader = "IncorrectIfMatchHeader";

            building.Initialize(new[]
            {
                buildingUnitPlanned
            });

            _mockBuildingsRepository
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(building));

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty));

            var request = new RealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act

            var result = (PreconditionFailedResult)await _controller.Realize(
                ResponseOptions,
                request,
                new ETag(ETagType.Strong, ifMatchHeader).ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenBuildingUnitNotFound_ThenValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitNotFoundException());

            var request = new RealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.Realize(
                ResponseOptions,
                request,
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Onbestaande gebouweenheid."
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenBuildingUnitIsRemoved_ThenValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException(buildingUnitPersistentLocalId));

            var request = new RealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.Realize(
                ResponseOptions,
                request,
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Verwijderde gebouweenheid."
                                   && x.StatusCode == StatusCodes.Status410Gone);
        }
    }
}
