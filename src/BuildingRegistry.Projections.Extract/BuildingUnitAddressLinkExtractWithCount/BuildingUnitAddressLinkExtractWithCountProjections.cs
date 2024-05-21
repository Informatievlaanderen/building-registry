namespace BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtractWithCount
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Extract gebouweenheidkoppelingen met adres")]
    [ConnectedProjectionDescription("Projectie die een extract voorziet voor gebouweenheid en adres koppelingen.")]
    public sealed class BuildingUnitAddressLinkExtractProjections : ConnectedProjection<ExtractContext>
    {
        public const string ObjectType = "Gebouweenheid";

        private readonly Encoding _encoding;

        public BuildingUnitAddressLinkExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits.Where(x => x.IsRemoved == false))
                {
                    foreach (var addressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                    {
                        await context
                            .BuildingUnitAddressLinkExtractWithCount
                            .AddAsync(new BuildingUnitAddressLinkExtractItem
                            {
                                BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                                BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                                AddressPersistentLocalId = addressPersistentLocalId,
                                DbaseRecord = new BuildingUnitAddressLinkDbaseRecord()
                                {
                                    objecttype = { Value = ObjectType },
                                    adresobjid = { Value = buildingUnit.BuildingUnitPersistentLocalId.ToString() },
                                    adresid = { Value = addressPersistentLocalId }
                                }.ToBytes(_encoding)
                            }, ct);
                    }
                }
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitAddressLinkExtractWithCount
                    .AddAsync(new BuildingUnitAddressLinkExtractItem
                    {
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                        AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                        DbaseRecord = new BuildingUnitAddressLinkDbaseRecord()
                        {
                            objecttype = { Value = ObjectType },
                            adresobjid = { Value = message.Message.BuildingUnitPersistentLocalId.ToString() },
                            adresid = { Value = message.Message.AddressPersistentLocalId }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await RemoveIdempotentLink(context,
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await RemoveIdempotentLink(context,
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await RemoveIdempotentLink(context,
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await RemoveIdempotentLink(context,
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var previousAddress =  await context.BuildingUnitAddressLinkExtractWithCount.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.PreviousAddressPersistentLocalId],
                    ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress =  await context.BuildingUnitAddressLinkExtractWithCount.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId],
                    ct);

                if (newAddress is null)
                {
                    await context.BuildingUnitAddressLinkExtractWithCount.AddAsync(
                        new BuildingUnitAddressLinkExtractItem
                        {
                            BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                            DbaseRecord = new BuildingUnitAddressLinkDbaseRecord
                            {
                                objecttype = { Value = ObjectType },
                                adresobjid = { Value = message.Message.BuildingUnitPersistentLocalId.ToString() },
                                adresid = { Value = message.Message.NewAddressPersistentLocalId }
                            }.ToBytes(_encoding)
                        },
                        ct);
                }
                else
                {
                    newAddress.Count += 1;
                }
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    foreach (var addressPersistentLocalId in buildingUnitReaddresses.DetachedAddressPersistentLocalIds)
                    {
                        await RemoveIdempotentLink(context,
                            new BuildingUnitPersistentLocalId(buildingUnitReaddresses.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(addressPersistentLocalId), ct);
                    }

                    foreach (var addressPersistentLocalId in buildingUnitReaddresses.AttachedAddressPersistentLocalIds)
                    {
                        await AddIdempotentLink(context, new BuildingUnitAddressLinkExtractItem
                        {
                            BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            BuildingUnitPersistentLocalId = buildingUnitReaddresses.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = addressPersistentLocalId,
                            DbaseRecord = new BuildingUnitAddressLinkDbaseRecord
                            {
                                objecttype = { Value = ObjectType },
                                adresobjid = { Value = buildingUnitReaddresses.BuildingUnitPersistentLocalId.ToString() },
                                adresid = { Value = addressPersistentLocalId }
                            }.ToBytes(_encoding)
                        }, ct);
                    }
                }
            });
        }

        private static async Task AddIdempotentLink(
            ExtractContext context,
            BuildingUnitAddressLinkExtractItem linkItem,
            CancellationToken ct)
        {
            var extractItem =  await context.BuildingUnitAddressLinkExtractWithCount.FindAsync(
                [linkItem.BuildingUnitPersistentLocalId, linkItem.AddressPersistentLocalId],
                ct);

            if (extractItem is null)
            {
                await context.BuildingUnitAddressLinkExtractWithCount.AddAsync(linkItem, ct);
            }
        }

        private static async Task RemoveIdempotentLink(
            ExtractContext context,
            int buildingUnitPersistentLocalId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var linkExtractItem =  await context.BuildingUnitAddressLinkExtractWithCount.FindAsync(
                [buildingUnitPersistentLocalId, addressPersistentLocalId],
                ct);

            if (linkExtractItem is not null)
            {
                context.Remove(linkExtractItem);
            }
        }
    }
}
