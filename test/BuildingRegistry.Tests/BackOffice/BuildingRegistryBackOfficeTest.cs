namespace BuildingRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit.Abstractions;

    public class BuildingRegistryBackOfficeTest : BuildingRegistryTest
    {
        internal const string BuildingDetailUrl = "https://www.registry.com/building/gepland/{0}";
        internal const string BuildingUnitDetailUrl = "https://www.registry.com/building/gepland/{0}";
        protected IOptions<ResponseOptions> ResponseOptions { get; }
        protected Mock<IMediator> MockMediator { get; }
        public const string Username = "John Doe";

        public BuildingRegistryBackOfficeTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            ResponseOptions = Options.Create(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.BuildingDetailUrl = BuildingDetailUrl;
            ResponseOptions.Value.BuildingUnitDetailUrl = BuildingUnitDetailUrl;
            MockMediator = new Mock<IMediator>();
        }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        public void PlanBuilding(BuildingPersistentLocalId buildingPersistentLocalId, ExtendedWkbGeometry wkbGeometry)
        {
            DispatchArrangeCommand(new PlanBuilding(buildingPersistentLocalId, wkbGeometry, Fixture.Create<Provenance>()));
        }

        public T CreateBuildingControllerWithUser<T>() where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object) as T;

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
            else
            {
                throw new Exception("Could not find controller type");
            }
        }

        public T CreateBuildingUnitControllerWithUser<T>(
            IBuildings buildingsRepository,
            BackOfficeContext backOfficeContext)
            where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object, buildingsRepository, backOfficeContext) as T;

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
            else
            {
                throw new Exception("Could not find controller type");
            }
        }
    }
}
