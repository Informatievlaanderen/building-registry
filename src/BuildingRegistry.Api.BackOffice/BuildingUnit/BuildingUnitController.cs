namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Api;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "gebouweenheden")]
    public partial class BuildingUnitController : BuildingRegistryController
    {
        private readonly IMediator _mediator;

        public BuildingUnitController(IMediator mediator)
            : base(mediator)
        {
            _mediator = mediator;
        }
    }
}
