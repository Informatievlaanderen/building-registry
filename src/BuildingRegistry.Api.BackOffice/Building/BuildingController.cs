namespace BuildingRegistry.Api.BackOffice.Building
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Infrastructure.FeatureToggles;
    using Infrastructure.Options;
    using MediatR;
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
            UseSqsToggle useSqsToggle,
            IOptions<TicketingOptions> ticketingOptions) : base(mediator, useSqsToggle, ticketingOptions)
        { }
    }
}
