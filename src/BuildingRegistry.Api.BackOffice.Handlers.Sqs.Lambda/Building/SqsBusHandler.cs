namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public abstract class SqsBusHandler
    {
        protected ITicketing Ticketing { get; }
        protected ICommandHandlerResolver Bus { get; }

        protected SqsBusHandler(ITicketing ticketing, ICommandHandlerResolver bus)
        {
            Ticketing = ticketing;
            Bus = bus;
        }

        protected async Task<long> IdempotentCommandHandlerDispatch(
            IdempotencyContext context,
            Guid? commandId,
            object command,
            IDictionary<string, object> metadata,
            CancellationToken cancellationToken)
        {
            if (!commandId.HasValue || command == null)
            {
                throw new ApiException("Ongeldig verzoek id.", StatusCodes.Status400BadRequest);
            }

            // First check if the command id already has been processed
            var possibleProcessedCommand = await context
                .ProcessedCommands
                .Where(x => x.CommandId == commandId)
                .ToDictionaryAsync(x => x.CommandContentHash, x => x, cancellationToken);

            var contentHash = SHA512
                .Create()
                .ComputeHash(Encoding.UTF8.GetBytes(command.ToString()!))
                .ToHexString();

            // It is possible we have a GUID collision, check the SHA-512 hash as well to see if it is really the same one.
            // Do nothing if commandId with contenthash exists
            if (possibleProcessedCommand.Any() && possibleProcessedCommand.ContainsKey(contentHash))
            {
                throw new IdempotencyException("Already processed");
            }

            var processedCommand = new ProcessedCommand(commandId.Value, contentHash);
            try
            {
                // Store commandId in Command Store if it does not exist
                await context.ProcessedCommands.AddAsync(processedCommand, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                // Do work
                return await Bus.Dispatch(commandId.Value,
                    command,
                    metadata,
                    cancellationToken);
            }
            catch
            {
                // On exception, remove commandId from Command Store
                context.ProcessedCommands.Remove(processedCommand);
                await context.SaveChangesAsync(cancellationToken);
                throw;
            }
        }

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

        protected async Task<string> GetBuildingHash(
            IBuildings buildings,
            BuildingPersistentLocalId buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), cancellationToken);
            return aggregate.LastEventHash;
        }
    }

    public class IdempotencyException : Exception
    {
        public IdempotencyException(string? message) : base(message)
        {
        }
    }
}
