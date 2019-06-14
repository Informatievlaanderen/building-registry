namespace BuildingRegistry.Projections.Legacy.BuildingDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Events;
    using GeoAPI.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using ValueObjects;

    public class BuildingDetailProjections : ConnectedProjection<LegacyContext>
    {
        private readonly WKBReader _wkbReader;

        public BuildingDetailProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingDetails
                    .AddAsync(
                        new BuildingDetailItem
                        {
                            BuildingId = message.Message.BuildingId
                        },
                        ct);
            });

            When<Envelope<BuildingOsloIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (item != null)
                    item.OsloId = message.Message.OsloId;
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.IsRemoved = true;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            #region Complete/Incomplete

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.IsComplete = true;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.IsComplete = false;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            #endregion

            #region Status

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.NotRealized;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Realized;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Retired;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.NotRealized;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Retired;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.Realized;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = null;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Status = null;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            #endregion

            #region Geometry

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.Geometry = null;
                item.GeometryMethod = null;
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkb, BuildingGeometryMethod.MeasuredByGrb);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkb, BuildingGeometryMethod.Outlined);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkb, BuildingGeometryMethod.MeasuredByGrb);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkb, BuildingGeometryMethod.Outlined);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            #endregion
        }

        private static void SetVersion(BuildingDetailItem item, Instant timestamp)
        {
            item.Version = timestamp;
        }

        private void SetGeometry(BuildingDetailItem item, string extendedWkb, BuildingGeometryMethod method)
        {
            var geometry = (IPolygon)_wkbReader.Read(extendedWkb.ToByteArray());

            item.Geometry = geometry;
            item.GeometryMethod = method;
        }
    }
}
