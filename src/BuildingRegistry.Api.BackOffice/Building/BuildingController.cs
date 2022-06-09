namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "gebouwen")]
    public partial class BuildingController : ApiBusController
    {
        private readonly IMediator _mediator;

        public BuildingController(IMediator mediator, ICommandHandlerResolver bus) : base(bus)
        {
            _mediator = mediator;
        }

        private ValidationException CreateValidationException(string errorCode, string propertyName, string message)
        {
            var failure = new ValidationFailure(propertyName, message)
            {
                ErrorCode = errorCode
            };

            return new ValidationException(new List<ValidationFailure>
            {
                failure
            });
        }
    }
}
