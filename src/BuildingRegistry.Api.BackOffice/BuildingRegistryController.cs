namespace BuildingRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using NodaTime;

    public class BuildingRegistryController : ApiController
    {
        protected IMediator Mediator { get; }
        protected IProvenanceFactory ProvenanceFactory { get; }

        private readonly TicketingOptions _ticketingOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BuildingRegistryController(
            IMediator mediator,
            IOptions<TicketingOptions> ticketingOptions,
            IHttpContextAccessor httpContextAccessor,
            IProvenanceFactory provenanceFactory)
        {
            Mediator = mediator;
            _ticketingOptions = ticketingOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            ProvenanceFactory = provenanceFactory;
        }

        protected IDictionary<string, object?> GetMetadata()
        {
            var correlationId = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["x-correlation-id"].FirstOrDefault() ?? Guid.NewGuid().ToString("D");

            return new Dictionary<string, object?>
            {
                { "CorrelationId", correlationId }
            };
        }

        protected Provenance CreateProvenance(Modification modification)
            => ProvenanceFactory.Create(new Reason(""), modification);

        protected Provenance CreateFakeProvenance()
        {
            return new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                Application.BuildingRegistry,
                new Reason(""), // TODO: TBD
                new Operator(""), // TODO: from claims
                Modification.Insert,
                Organisation.DigitaalVlaanderen // TODO: from claims
            );
        }

        protected IActionResult Accepted(LocationResult locationResult)
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
