namespace BuildingRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Building;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;

    public class BuildingRegistryController : ApiController
    {
        protected IMediator Mediator { get; }

        private readonly TicketingOptions _ticketingOptions;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IProvenanceFactory _provenanceFactory;

        public BuildingRegistryController(
            IMediator mediator,
            IOptions<TicketingOptions> ticketingOptions,
            IActionContextAccessor actionContextAccessor,
            IProvenanceFactory provenanceFactory)
        {
            Mediator = mediator;
            _actionContextAccessor = actionContextAccessor;
            _provenanceFactory = provenanceFactory;
            _ticketingOptions = ticketingOptions.Value;
        }

        protected IDictionary<string, object> GetMetadata()
        {
            var correlationId = _actionContextAccessor
                .ActionContext?
                .HttpContext
                .Request
                .Headers["x-correlation-id"].FirstOrDefault() ?? Guid.NewGuid().ToString("D");

            return new Dictionary<string, object>
            {
                { "CorrelationId", correlationId }
            };
        }

        protected Provenance CreateProvenance(Modification modification)
            => _provenanceFactory.Create(new Reason(""), modification);

        protected Provenance CreateFakeProvenance()
        {
            return new Provenance(
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                Application.BuildingRegistry,
                new Reason(""), // TODO: TBD
                new Operator(""), // TODO: from claims
                Modification.Insert,
                Organisation.DigitaalVlaanderen // TODO: from claims
            );
        }

        public IActionResult Accepted(LocationResult locationResult)
        {
            return Accepted(locationResult
                .Location
                .ToString()
                .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl));
        }

        protected ValidationException CreateValidationException(string errorCode, string propertyName, string message)
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
