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
                var parcelId = Guid.Parse(message.ParcelId);
                var parcel = await context
                    .ParcelConsumerItems.FindAsync(new object?[] { parcelId }, cancellationToken: ct);

                if (parcel is null)
                {
                    var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    await context
                        .ParcelConsumerItems
                        .AddAsync(new ParcelConsumerItem(
                                parcelId,
                                message.CaPaKey,
                                ParcelStatus.Parse(message.ParcelStatus),
                                extendedWkbGeometry,
                                wkbReader.Read(extendedWkbGeometry),
                                message.IsRemoved)
                            , ct);

                    foreach (var addressPersistentLocalId in message.AddressPersistentLocalIds)
                    {
                        await context.AddIdempotentParcelAddress(parcelId, addressPersistentLocalId, ct);
                    }
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
                parcel.SetGeometry(wkbReader.Read(extendedWkbGeometry));
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
                                wkbReader.Read(extendedWkbGeometry))
                            , ct);
                }
            });

            When<ParcelAddressWasAttachedV2>(async (context, message, ct) =>
            {
                await context.AddIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.AddressPersistentLocalId, ct);
            });

            When<ParcelAddressWasDetachedBecauseAddressWasRejected>(async (context, message, ct) =>
            {
                await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.AddressPersistentLocalId, ct);
            });

            When<ParcelAddressWasDetachedBecauseAddressWasRemoved>(async (context, message, ct) =>
            {
                await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.AddressPersistentLocalId, ct);
            });

            When<ParcelAddressWasDetachedBecauseAddressWasRetired>(async (context, message, ct) =>
            {
                await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.AddressPersistentLocalId, ct);
            });

            When<ParcelAddressWasDetachedV2>(async (context, message, ct) =>
            {
                await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.AddressPersistentLocalId, ct);
            });

            When<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(async (context, message, ct) =>
            {
                await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.PreviousAddressPersistentLocalId, ct);
                await context.AddIdempotentParcelAddress(Guid.Parse(message.ParcelId), message.NewAddressPersistentLocalId, ct);
            });

            When<ParcelAddressesWereReaddressed>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.DetachedAddressPersistentLocalIds)
                {
                    await context.RemoveIdempotentParcelAddress(Guid.Parse(message.ParcelId), addressPersistentLocalId, ct);
                }

                foreach (var addressPersistentLocalId in message.AttachedAddressPersistentLocalIds)
                {
                    await context.AddIdempotentParcelAddress(Guid.Parse(message.ParcelId), addressPersistentLocalId, ct);
                }
            });
        }
    }
}
