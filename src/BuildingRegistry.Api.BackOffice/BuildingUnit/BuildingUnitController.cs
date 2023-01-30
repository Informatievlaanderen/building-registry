namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "gebouweenheden")]
    public partial class BuildingUnitController : BuildingRegistryController
    {
        public BuildingUnitController(
            IMediator mediator,
            IOptions<TicketingOptions> ticketingOptions) : base(mediator, ticketingOptions)
        { }
    }
}
