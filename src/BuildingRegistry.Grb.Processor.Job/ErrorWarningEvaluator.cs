namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using TicketingService.Abstractions;

    public class ErrorWarningEvaluator
    {
        public static readonly IReadOnlyCollection<string> Warnings = new[]
        {
            "VerwijderdGebouw"
        };

        public (JobRecordStatus jobRecordStatus, string message) Evaluate(IEnumerable<ValidationError> validationErrors)
        {
            var errors = validationErrors!
                .Where(x => x.Code is not null && !Warnings.Contains(x.Code))
                .ToList();

            return errors.Any()
                ? (JobRecordStatus.Error,
                    errors.Select(x => x.Reason).Aggregate((result, error) => $"{result}{Environment.NewLine}{error}"))
                : (JobRecordStatus.Warning, validationErrors!
                    .Select(x => x.Reason)
                    .Aggregate((warning, result) => $"{result}{Environment.NewLine}{warning}"));
        }

        public (JobRecordStatus jobRecordStatus, string message) Evaluate(TicketError ticketError)
        {
            return Warnings.Contains(ticketError.ErrorCode)
                ? (JobRecordStatus.Warning, ticketError.ErrorMessage)
                : (JobRecordStatus.Error, ticketError.ErrorMessage);
        }
    }
}
