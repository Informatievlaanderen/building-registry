namespace BuildingRegistry.Api.CrabImport.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Security.Claims;

    public abstract class ApiController : ControllerBase
    {
        protected IDictionary<string, object> GetMetadata()
        {
            var ip = User.FindFirst(AddRemoteIpAddressMiddleware.UrnBasisregistersVlaanderenIp)?.Value;
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst("urn:be:vlaanderen:gebouwregister:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)
                ?.Value;

            return new Dictionary<string, object>
            {
                {"FirstName", firstName},
                {"LastName", lastName},
                {"Ip", ip},
                {"UserId", userId},
                {"CorrelationId", correlationId}
            };
        }
    }

    public abstract class ApiBusController : ApiController
    {
        protected ICommandHandlerResolver Bus { get; }

        protected ApiBusController(ICommandHandlerResolver bus) => Bus = bus;
    }
}
