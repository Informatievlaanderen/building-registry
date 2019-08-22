namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Events.Crab;
    using GeoAPI.Geometries;
    using Microsoft.Extensions.Options;

    public class BuildingUnitExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";

        private const string Unknown = "NietGekend";
        private const string Common = "GemeenschappelijkDeel";

        private const string DerivedFromObject = "AfgeleidVanObject";
        private const string AppointedByAdministrator = "AangeduidDoorBeheerder";

        private readonly ExtractConfig _extractConfig;
        private readonly Encoding _encoding;

        public BuildingUnitExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _extractConfig = extractConfig.Value;
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var units = context.BuildingUnitExtract.Local.Where(x => x.BuildingId == message.Message.BuildingId)
                    .Union(context.BuildingUnitExtract.Where(x => x.BuildingId == message.Message.BuildingId));

                foreach (var unit in units)
                    UpdateRecord(unit, item => { item.gebouwid.Value = message.Message.PersistentLocalId.ToString(); });
            });

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
                            functie = { Value = Common }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
                            functie = { Value = Unknown }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
                            functie = { Value = Unknown }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
                            functie = { Value = Unknown }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingUnitExtract.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                context.BuildingUnitExtract.Remove(item);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.PersistentLocalId = message.Message.PersistentLocalId;
                        UpdateId(item, message.Message.PersistentLocalId);
                    }, ct);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.IsComplete = true;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.IsComplete = false;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, AppointedByAdministrator);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, AppointedByAdministrator);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, DerivedFromObject);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, DerivedFromObject);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, null);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByBuilding>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => DoNothing());

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRetired>>(async (context, message, ct) => DoNothing());

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

        private static void UpdateGeometry(BuildingUnitExtractItem item, IGeometry geometry)
        {
            var pointShapeContent = new PointShapeContent(new PointM(geometry.Coordinate));
            item.ShapeRecordContent = pointShapeContent.ToBytes();
            item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
            var env = EnvelopePartialRecord.From(pointShapeContent.Shape.EnvelopeInternal);
            item.MinimumX = env.MinimumX;
            item.MaximumX = env.MaximumX;
            item.MinimumY = env.MinimumY;
            item.MaximumY = env.MaximumY;
        }

        private void UpdateStatus(BuildingUnitExtractItem buildingUnit, string status)
            => UpdateRecord(buildingUnit, record => record.status.Value = status);

        private void UpdateGeometryMethod(BuildingUnitExtractItem buildingUnit, string method)
            => UpdateRecord(buildingUnit, record => record.posgeommet.Value = method);

        private void UpdateId(BuildingUnitExtractItem buildingUnit, int id)
            => UpdateRecord(buildingUnit, record =>
            {
                record.id.Value = $"{_extractConfig.DataVlaanderenNamespaceBuildingUnit}/{id}";
                record.gebouwehid.Value = id;
            });

        private void UpdateVersie(BuildingUnitExtractItem buildingUnit, Instant timestamp)
            => UpdateRecord(buildingUnit, record => record.versieid.Value = timestamp.ToBelgianDateTimeOffset().DateTime);

        private void UpdateRecord(BuildingUnitExtractItem buildingUnit, Action<BuildingUnitDbaseRecord> updateFunc)
        {
            var record = new BuildingUnitDbaseRecord();
            record.FromBytes(buildingUnit.DbaseRecord, _encoding);

            updateFunc(record);

            buildingUnit.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}
