namespace BuildingRegistry.Tests.BackOffice.Api.WhenChangingBuildingOutline
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingHasBuildingUnitsOutsideBuildingGeometry : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingHasBuildingUnitsOutsideBuildingGeometry(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingOutlineRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasBuildingUnitsOutsideBuildingGeometryException());

            //Act
            Func<Task> act = async () => await _controller.ChangeOutline(
                ResponseOptions,
                MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                null,
                MockIfMatchValidator(true),
                buildingPersistentLocalId,
                new ChangeBuildingOutlineRequest(),
                null,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "GebouweenheidGeomtrieBuitenGebouwGeometrie"
                                   && failure.ErrorMessage == "Het gebouw heeft onderliggende gebouweenheden buiten de nieuw geschetste gebouwgeometrie."));
        }
    }
}
