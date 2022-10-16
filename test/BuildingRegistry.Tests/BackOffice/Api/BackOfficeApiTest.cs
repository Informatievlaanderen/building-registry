namespace BuildingRegistry.Tests.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api;
    using Building;
    using BuildingRegistry.Api.BackOffice.Infrastructure;
    using BuildingRegistry.Api.BackOffice.Infrastructure.FeatureToggles;
    using BuildingRegistry.Api.BackOffice.Infrastructure.Options;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit.Abstractions;

    public class BackOfficeApiTest : BuildingRegistryTest
    {
        internal const string BuildingDetailUrl = "https://www.registry.com/building/gepland/{0}";
        internal const string BuildingUnitDetailUrl = "https://www.registry.com/buildingunit/gepland/{0}";

        private const string PublicTicketUrl = "https://www.ticketing.com";
        private const string InternalTicketUrl = "https://www.internalticketing.com";

        protected IOptions<ResponseOptions> ResponseOptions { get; }
        private IOptions<TicketingOptions> TicketingOptions { get; }
        protected Mock<IMediator> MockMediator { get; }

        private const string Username = "John Doe";

        protected BackOfficeApiTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            ResponseOptions = Options.Create(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.BuildingDetailUrl = BuildingDetailUrl;
            ResponseOptions.Value.BuildingUnitDetailUrl = BuildingUnitDetailUrl;

            TicketingOptions = Options.Create(Fixture.Create<TicketingOptions>());
            TicketingOptions.Value.PublicBaseUrl = PublicTicketUrl;
            TicketingOptions.Value.InternalBaseUrl = InternalTicketUrl;

            MockMediator = new Mock<IMediator>();
        }

        protected IIfMatchHeaderValidator MockIfMatchValidator(bool expectedResult)
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();
            mockIfMatchHeaderValidator
                .Setup(x =>
                    x.IsValidForBuildingUnit(It.IsAny<string>(), It.IsAny<BuildingUnitPersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));

            mockIfMatchHeaderValidator
                .Setup(x =>
                    x.IsValidForBuilding(It.IsAny<string>(), It.IsAny<BuildingPersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));

            return mockIfMatchHeaderValidator.Object;
        }

        protected IValidator<TRequest> MockValidRequestValidator<TRequest>()
        {
            var mockRequestValidator = new Mock<IValidator<TRequest>>();

            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            return mockRequestValidator.Object;
        }

        protected Uri CreateTicketUri(Guid ticketId)
        {
            return new Uri($"{InternalTicketUrl}/tickets/{ticketId:D}");
        }

        protected void AssertLocation(string? location, Guid ticketId)
        {
            var expectedLocation = $"{PublicTicketUrl}/tickets/{ticketId:D}";

            location.Should().NotBeNullOrWhiteSpace();
            location.Should().Be(expectedLocation);
        }

        protected T CreateBuildingControllerWithUser<T>(bool useSqs = false) where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object, new UseSqsToggle(useSqs), TicketingOptions) as T;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", Username),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            if (controller != null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

                return controller;
            }

            throw new Exception("Could not find controller type");
        }

        protected T CreateBuildingUnitControllerWithUser<T>(bool useSqs = false) where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object, new UseSqsToggle(useSqs), TicketingOptions) as T;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", Username),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            if (controller != null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

                return controller;
            }

            throw new Exception("Could not find controller type");
        }
    }
}
