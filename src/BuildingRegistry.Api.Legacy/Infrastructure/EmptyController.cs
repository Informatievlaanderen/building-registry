namespace BuildingRegistry.Api.Legacy.Infrastructure
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.Api;

    [ApiVersionNeutral]
    [Route("")]
    public class StatusController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
        {
            return Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the VBR Building Api v{Assembly.GetEntryAssembly().GetName().Version}.");
        }
    }
}
