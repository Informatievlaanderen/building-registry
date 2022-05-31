namespace BuildingRegistry.Consumer.Address.Projections
{
    using System;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public class AddressKafkaProjection : ConnectedProjection<ConsumerAddressContext>
    {
        public AddressKafkaProjection()
        {
            When<AddressWasMigratedToStreetName>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            Guid.Parse(message.AddressId),
                            AddressStatus.Parse(message.Status),
                            message.IsRemoved)
                        , ct);
            });

            When<AddressWasProposedV2>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed)
                        , ct);
            });

            When<AddressWasApproved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });
        }
    }
}
