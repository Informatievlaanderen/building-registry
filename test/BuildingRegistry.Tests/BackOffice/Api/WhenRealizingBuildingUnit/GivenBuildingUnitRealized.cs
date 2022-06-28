namespace BuildingRegistry.Tests.BackOffice.Api.WhenRealizingBuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Building;
    using Building.Events;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Legacy.Events;
    using FeatureToggle;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitRealized : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingUnitController _controller;

        private readonly Mock<IBuildings> _mockBuildingsRepository;
        private readonly BackOfficeContext _backOfficeContext;

        public GivenBuildingUnitRealized(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockBuildingsRepository = new Mock<IBuildings>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(_mockBuildingsRepository.Object, _backOfficeContext);
        }

        [Fact]
        public async Task WithoutIfMatchHeader_ThenShouldSucceed()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty));

            var request = new RealizeBuildingUnitRequest()
            {
                PersistentLocalId = buildingUnitPersistentLocalId
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
        public async Task WithCorrectIfMatchHeader_ThenShouldSucceed()
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
                PersistentLocalId = buildingUnitPersistentLocalId
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
        public async Task WithInvalidIfMatchHeader_ThenShouldSucceed()
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
                PersistentLocalId = buildingUnitPersistentLocalId
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
    }
}
