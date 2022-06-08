namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "gebouwen")]
    public partial class BuildingController : ApiBusController
    {
        public BuildingController(ICommandHandlerResolver bus) : base(bus) { }

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

        private Provenance CreateFakeProvenance()
        {
            return new Provenance(
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                Application.StreetNameRegistry,
                new Reason(""), // TODO: TBD
                new Operator(""), // TODO: from claims
                Modification.Insert,
                Organisation.DigitaalVlaanderen // TODO: from claims
            );
        }
    }
}
