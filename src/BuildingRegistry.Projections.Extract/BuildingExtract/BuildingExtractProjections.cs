namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Events;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Text;

    public class BuildingExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";
        private const string UnderConstruction = "InAanbouw";

        private const string MeasuredByGrb = "IngemetenGRB";
        private const string Outlined = "Ingeschetst";

        private const string IdUri = "https://data.vlaanderen.be/id/gebouw";

        private readonly Encoding _encoding;

        public BuildingExtractProjections(Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingExtract
                    .AddAsync(new BuildingExtractItem
                    {
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        DbaseRecord = new BuildingDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        item.PersistentLocalId = message.Message.PersistentLocalId;
                        UpdateId(item, message.Message.PersistentLocalId);
                    }, ct);
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        item.IsComplete = true;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        item.IsComplete = false;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, UnderConstruction);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateGeometryMethod(item, null);
                        item.ShapeRecordContent = null;
                        item.ShapeRecordContentLength = 0;
                        item.MaximumX = 0;
                        item.MinimumX = 0;
                        item.MaximumY = 0;
                        item.MinimumY = 0;

                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateGeometryMethod(item, MeasuredByGrb);
                        var geometry = wkbReader.Read(message.Message.ExtendedWkb.ToByteArray()) as Polygon;
                        UpdateGeometry(geometry, item);

                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateGeometryMethod(item, Outlined);
                        var geometry = wkbReader.Read(message.Message.ExtendedWkb.ToByteArray()) as Polygon;
                        UpdateGeometry(geometry, item);

                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, null);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, null);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, UnderConstruction);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateGeometryMethod(item, MeasuredByGrb);
                        var geometry = wkbReader.Read(message.Message.ExtendedWkb.ToByteArray()) as Polygon;
                        UpdateGeometry(geometry, item);

                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateGeometryMethod(item, Outlined);
                        var geometry = wkbReader.Read(message.Message.ExtendedWkb.ToByteArray()) as Polygon;
                        UpdateGeometry(geometry, item);

                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingExtract(message.Message.BuildingId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtract.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                context.BuildingExtract.Remove(item);
            });
        }

        private static void UpdateGeometry(Polygon geometry, BuildingExtractItem item)
        {
            var polygonShapeContent = new Be.Vlaanderen.Basisregisters.Shaperon.PolygonShapeContent(geometry);
            item.ShapeRecordContent = polygonShapeContent.ToBytes();
            item.ShapeRecordContentLength = polygonShapeContent.ContentLength.ToInt32();
            var env = EnvelopePartialRecord.From(polygonShapeContent.Shape.EnvelopeInternal);
            item.MinimumX = env.MinimumX;
            item.MaximumX = env.MaximumX;
            item.MinimumY = env.MinimumY;
            item.MaximumY = env.MaximumY;
        }

        private void UpdateStatus(BuildingExtractItem building, string status)
            => UpdateRecord(building, record => record.status.Value = status);

        private void UpdateGeometryMethod(BuildingExtractItem building, string method)
            => UpdateRecord(building, record => record.geommet.Value = method);

        private void UpdateId(BuildingExtractItem building, int id)
            => UpdateRecord(building, record =>
            {
                record.id.Value = $"{IdUri}/{id}";
                record.gebouwid.Value = id;
            });

        private void UpdateVersie(BuildingExtractItem building, Instant timestamp)
            => UpdateRecord(building, record => record.versieid.Value = timestamp.ToBelgianDateTimeOffset().DateTime);

        private void UpdateRecord(BuildingExtractItem building, Action<BuildingDbaseRecord> updateFunc)
        {
            var record = new BuildingDbaseRecord();
            record.FromBytes(building.DbaseRecord, _encoding);

            updateFunc(record);

            building.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
