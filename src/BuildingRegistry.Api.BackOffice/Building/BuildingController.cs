namespace BuildingRegistry.Api.BackOffice.Building
{
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "gebouwen")]
    public partial class BuildingController : BuildingRegistryController
    {
        public BuildingController(
            IMediator mediator,
            IOptions<TicketingOptions> ticketingOptions,
            IHttpContextAccessor httpContextAccessor,
            IProvenanceFactory provenanceFactory)
            : base(mediator, ticketingOptions, httpContextAccessor, provenanceFactory)
        { }
    }
}
