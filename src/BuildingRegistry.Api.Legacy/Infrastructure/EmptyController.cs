namespace BuildingRegistry.Api.Legacy.Infrastructure
{
    using System.Reflection;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Basisregisters Vlaanderen Building Legacy Api {Assembly.GetEntryAssembly().GetVersionText()}.");
    }
}
