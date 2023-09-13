namespace BuildingRegistry.Consumer.Read.Parcel.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;

    public class ParcelKafkaProjection : ConnectedProjection<ConsumerParcelContext>
    {
        public ParcelKafkaProjection()
        {
            var wkbReader = WKBReaderFactory.Create();

            When<ParcelWasMigrated>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { Guid.Parse(message.ParcelId) }, cancellationToken: ct);

                if (parcel is null)
                {
                    var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    await context
                        .ParcelConsumerItems
                        .AddAsync(new ParcelConsumerItem(
                                Guid.Parse(message.ParcelId),
                                message.CaPaKey,
                                ParcelStatus.Parse(message.ParcelStatus),
                                extendedWkbGeometry,
                                wkbReader.Read(extendedWkbGeometry),
                                message.IsRemoved)
                            , ct);
                }
            });

            When<ParcelWasRetiredV2>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { Guid.Parse(message.ParcelId) }, cancellationToken: ct);

                parcel!.Status = ParcelStatus.Retired;
            });

            When<ParcelWasCorrectedFromRetiredToRealized>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { Guid.Parse(message.ParcelId) }, cancellationToken: ct);

                parcel!.Status = ParcelStatus.Realized;
            });

            When<ParcelGeometryWasChanged>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { Guid.Parse(message.ParcelId) }, cancellationToken: ct);

                var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                parcel!.ExtendedWkbGeometry = extendedWkbGeometry;
                parcel.Geometry = wkbReader.Read(extendedWkbGeometry);
            });

            When<ParcelWasImported>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { Guid.Parse(message.ParcelId) }, cancellationToken: ct);

                if (parcel is null)
                {
                    var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    await context
                        .ParcelConsumerItems
                        .AddAsync(new ParcelConsumerItem(
                                Guid.Parse(message.ParcelId),
                                message.CaPaKey,
                                ParcelStatus.Realized,
                                extendedWkbGeometry,
                                wkbReader.Read(extendedWkbGeometry),
                                false)
                            , ct);
                }
            });
        }
    }
}
