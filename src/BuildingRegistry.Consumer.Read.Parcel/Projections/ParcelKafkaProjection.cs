namespace BuildingRegistry.Consumer.Read.Parcel.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public class ParcelKafkaProjection : ConnectedProjection<ConsumerParcelContext>
    {
        public ParcelKafkaProjection()
        {
            When<ParcelWasMigrated>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new[] { Guid.Parse(message.ParcelId) }, ct);

                if (parcel is not null)
                {
                    await context
                        .ParcelConsumerItems
                        .AddAsync(new ParcelConsumerItem(
                                Guid.Parse(message.ParcelId),
                                message.CaPaKey,
                                ParcelStatus.Parse(message.ParcelStatus),
                                message.IsRemoved)
                            , ct);
                }
            });
        }
    }
}
