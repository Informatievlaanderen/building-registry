namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Legacy.Events;
    using Legacy.Events.Crab;
    using Microsoft.Extensions.Options;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    [ConnectedProjectionName("Extract gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen extract voorziet.")]
    public class BuildingExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";
        private const string UnderConstruction = "InAanbouw";

        private const string MeasuredByGrb = "IngemetenGRB";
        private const string Outlined = "Ingeschetst";

        private readonly ExtractConfig _extractConfig;
        private readonly Encoding _encoding;

        public BuildingExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _extractConfig = extractConfig.Value;
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
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
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
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
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
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
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
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
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
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
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

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());

            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void UpdateGeometry(Polygon geometry, BuildingExtractItem item)
        {
            if (geometry == null)
                item.ShapeRecordContentLength = 0;
            else
            {
                var env = EnvelopePartialRecord.From(geometry.EnvelopeInternal);

                var polygon =
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPolygon(geometry);
                var polygonShapeContent = new PolygonShapeContent(polygon);
                item.ShapeRecordContent = polygonShapeContent.ToBytes();
                item.ShapeRecordContentLength = polygonShapeContent.ContentLength.ToInt32();

                item.MinimumX = env.MinimumX;
                item.MaximumX = env.MaximumX;
                item.MinimumY = env.MinimumY;
                item.MaximumY = env.MaximumY;
            }
        }

        private void UpdateStatus(BuildingExtractItem building, string status)
            => UpdateRecord(building, record => record.status.Value = status);

        private void UpdateGeometryMethod(BuildingExtractItem building, string method)
            => UpdateRecord(building, record => record.geommet.Value = method);

        private void UpdateId(BuildingExtractItem building, int id)
            => UpdateRecord(building, record =>
            {
                record.id.Value = $"{_extractConfig.DataVlaanderenNamespaceBuilding}/{id}";
                record.gebouwid.Value = id;
            });

        private void UpdateVersie(BuildingExtractItem building, Instant timestamp)
            => UpdateRecord(building, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(BuildingExtractItem building, Action<BuildingDbaseRecord> updateFunc)
        {
            var record = new BuildingDbaseRecord();
            record.FromBytes(building.DbaseRecord, _encoding);

            updateFunc(record);

            building.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}
