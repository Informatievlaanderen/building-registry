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
    using BuildingRegistry.Api.BackOffice.Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit.Abstractions;

    public class BuildingRegistryBackOfficeTest : BuildingRegistryTest
    {
        internal const string DetailUrl = "https://www.registry.com/buliding/gepland/{0}";
        protected IOptions<ResponseOptions> ResponseOptions { get; }
        protected Mock<IMediator> MockMediator { get; }

        public BuildingRegistryBackOfficeTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            ResponseOptions = Options.Create<ResponseOptions>(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.BuildingDetailUrl = DetailUrl;
            MockMediator = new Mock<IMediator>();
        }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        public T CreateApiBusControllerWithUser<T>(string username) where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object) as T;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", username),
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
