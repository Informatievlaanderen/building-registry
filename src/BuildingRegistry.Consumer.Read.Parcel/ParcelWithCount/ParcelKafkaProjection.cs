namespace BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using Projections.Legacy;

    public class ParcelKafkaProjection : ConnectedProjection<ConsumerParcelContext>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly Lambert2008ConversionCompletedToggle _conversionCompleted;

        public ParcelKafkaProjection(
            ILifetimeScope lifetimeScope,
            Lambert2008ConversionCompletedToggle conversionCompleted)
        {
            _lifetimeScope = lifetimeScope;
            _conversionCompleted = conversionCompleted;

            When<ParcelWasMigrated>(async (context, message, ct) =>
            {
                var parcelId = Guid.Parse(message.ParcelId);
                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([parcelId], cancellationToken: ct);

                if (parcel is null)
                {
                    var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    var geometry = ReadGeometry(extendedWkbGeometry);

                    await context
                        .ParcelConsumerItemsWithCount
                        .AddAsync(new ParcelConsumerItem(
                                parcelId,
                                message.CaPaKey,
                                ParcelStatus.Parse(message.ParcelStatus),
                                extendedWkbGeometry,
                                geometry,
                                message.IsRemoved)
                            , ct);

                    foreach (var addressPersistentLocalId in message.AddressPersistentLocalIds)
                    {
                        await context.AddIdempotentParcelAddress(parcelId, addressPersistentLocalId, ct);
                    }

                    var buildingPersistentLocalIds = await GetBuildingPersistentLocalIdsToInvalidate(geometry);
                    context.BuildingsToInvalidate.AddRange(buildingPersistentLocalIds.Select(x => new BuildingToInvalidate
                    {
                        BuildingPersistentLocalId = x
                    }));
                }
            });

            When<ParcelWasRetiredV2>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([Guid.Parse(message.ParcelId)], cancellationToken: ct);

                parcel!.Status = ParcelStatus.Retired;

                var buildingPersistentLocalIds = await GetBuildingPersistentLocalIdsToInvalidate(parcel.GeometryIn(_conversionCompleted.MatchingSrid)!);
                context.BuildingsToInvalidate.AddRange(buildingPersistentLocalIds.Select(x => new BuildingToInvalidate
                {
                    BuildingPersistentLocalId = x
                }));
            });

            When<ParcelWasCorrectedFromRetiredToRealized>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([Guid.Parse(message.ParcelId)], cancellationToken: ct);

                parcel!.Status = ParcelStatus.Realized;
                var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                parcel.ExtendedWkbGeometry = extendedWkbGeometry;
                parcel.SetGeometry(ReadGeometry(extendedWkbGeometry));

                var buildingPersistentLocalIds = await GetBuildingPersistentLocalIdsToInvalidate(parcel.GeometryIn(_conversionCompleted.MatchingSrid)!);

                context.BuildingsToInvalidate.AddRange(buildingPersistentLocalIds.Select(x => new BuildingToInvalidate
                {
                    BuildingPersistentLocalId = x
                }));
            });

            When<ParcelGeometryWasChanged>(async (context, message, ct) =>
            {
                await using var scope = lifetimeScope.BeginLifetimeScope();
                var buildingMatching = scope.Resolve<IBuildingMatching>();

                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([Guid.Parse(message.ParcelId)], cancellationToken: ct);

                var previousBuildingPersistentLocalIds = buildingMatching.GetUnderlyingBuildings(parcel!.GeometryIn(_conversionCompleted.MatchingSrid)!).ToArray();

                var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                parcel.ExtendedWkbGeometry = extendedWkbGeometry;
                parcel.SetGeometry(ReadGeometry(extendedWkbGeometry));

                var currentBuildingPersistentLocalIds = buildingMatching.GetUnderlyingBuildings(parcel.GeometryIn(_conversionCompleted.MatchingSrid)!).ToArray();

                var buildingPersistentLocalIds = previousBuildingPersistentLocalIds
                    .Except(currentBuildingPersistentLocalIds)
                    .Union(currentBuildingPersistentLocalIds.Except(previousBuildingPersistentLocalIds));

                context.BuildingsToInvalidate.AddRange(buildingPersistentLocalIds.Select(x => new BuildingToInvalidate
                {
                    BuildingPersistentLocalId = x
                }));
            });

            // The parcel register's conversion to Lambert 2008. It re-expresses the geometry rather than
            // moving it, so Geometry keeps its exact Lambert 72 value and only the Lambert 2008 column is
            // written. Because nothing moves, no building's parcel membership changes and nothing is
            // invalidated. See ADR 0006.
            When<ParcelGeometryCrsWasChanged>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([Guid.Parse(message.ParcelId)], cancellationToken: ct);

                var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                parcel!.ExtendedWkbGeometry = extendedWkbGeometry;
                parcel.SetGeometryFromCrsConversion(ReadGeometry(extendedWkbGeometry));
            });

            When<ParcelWasImported>(async (context, message, ct) =>
            {
                var parcel = await context
                    .ParcelConsumerItemsWithCount.FindAsync([Guid.Parse(message.ParcelId)], cancellationToken: ct);

                if (parcel is null)
                {
                    var extendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    var geometry = ReadGeometry(extendedWkbGeometry);

                    await context
                        .ParcelConsumerItemsWithCount
                        .AddAsync(new ParcelConsumerItem(
                                Guid.Parse(message.ParcelId),
                                message.CaPaKey,
                                ParcelStatus.Realized,
                                extendedWkbGeometry,
                                geometry)
                            , ct);

                    var buildingPersistentLocalIds = await GetBuildingPersistentLocalIdsToInvalidate(geometry);
                    context.BuildingsToInvalidate.AddRange(buildingPersistentLocalIds.Select(x => new BuildingToInvalidate
                    {
                        BuildingPersistentLocalId = x
                    }));
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
                var previousRelation =
                    await context.ParcelAddressItemsWithCount.FindAsync(
                        [Guid.Parse(message.ParcelId), message.PreviousAddressPersistentLocalId],
                        cancellationToken: ct);

                if (previousRelation is not null && previousRelation.Count > 1)
                {
                    previousRelation.Count -= 1;
                }
                else if (previousRelation is not null)
                {
                    context.ParcelAddressItemsWithCount.Remove(previousRelation);
                }

                var newRelation =
                    await context.ParcelAddressItemsWithCount.FindAsync(
                        [Guid.Parse(message.ParcelId), message.NewAddressPersistentLocalId],
                        cancellationToken: ct);

                if (newRelation is null)
                {
                    context.ParcelAddressItemsWithCount.Add(new ParcelAddressItem(
                        Guid.Parse(message.ParcelId), message.NewAddressPersistentLocalId));
                }
                else
                {
                    newRelation.Count += 1;
                }
            });

            When<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>(async (context, message, ct) =>
            {
                var previousRelation =
                    await context.ParcelAddressItemsWithCount.FindAsync(
                        [Guid.Parse(message.ParcelId), message.PreviousAddressPersistentLocalId],
                        cancellationToken: ct);

                if (previousRelation is not null)
                {
                    context.ParcelAddressItemsWithCount.Remove(previousRelation);
                }

                var newRelation =
                    await context.ParcelAddressItemsWithCount.FindAsync(
                        [Guid.Parse(message.ParcelId), message.NewAddressPersistentLocalId],
                        cancellationToken: ct);

                if (newRelation is null)
                {
                    context.ParcelAddressItemsWithCount.Add(new ParcelAddressItem(
                        Guid.Parse(message.ParcelId), message.NewAddressPersistentLocalId));
                }
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

        /// <summary>
        /// Reads a geometry in whatever reference system the EWKB carries, rather than assuming one.
        /// Geometries persisted before the event store wrote EWKB carry no SRID and are read as Lambert 72.
        /// </summary>
        private static Geometry ReadGeometry(byte[] extendedWkbGeometry)
            => WKBReaderFactory.CreateForEwkb(extendedWkbGeometry).Read(extendedWkbGeometry);

        private async Task<IEnumerable<int>> GetBuildingPersistentLocalIdsToInvalidate(Geometry geometry)
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope();
            var buildingMatching = scope.Resolve<IBuildingMatching>();

            var buildingPersistentLocalIds = buildingMatching.GetUnderlyingBuildings(geometry);
            return buildingPersistentLocalIds;
        }
    }
}
