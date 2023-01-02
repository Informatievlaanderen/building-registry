namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;
    using NodaTime;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    [ConnectedProjectionName("Extract gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen extract voorziet.")]
    public class BuildingExtractV2Projections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";
        private const string UnderConstruction = "InAanbouw";

        private const string MeasuredByGrb = "IngemetenGRB";
        private const string Outlined = "Ingeschetst";

        private readonly Encoding _encoding;

        public BuildingExtractV2Projections(IOptions<ExtractConfig> extractConfig, Encoding encoding,
            WKBReader wkbReader)
        {
            var extractConfigValue = extractConfig.Value;
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                {
                    return;
                }

                var buildingExtractItemV2 = new BuildingExtractItemV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    DbaseRecord = new BuildingDbaseRecord
                    {
                        id =
                        {
                            Value =
                                $"{extractConfigValue.DataVlaanderenNamespaceBuilding}/{message.Message.BuildingPersistentLocalId}"
                        },
                        gebouwid = {Value = message.Message.BuildingPersistentLocalId},
                        geommet =
                        {
                            Value = MapGeometryMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod))
                        },
                        status = {Value = MapStatus(BuildingStatus.Parse(message.Message.BuildingStatus))},
                        versieid =
                        {
                            Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset()
                        }
                    }.ToBytes(_encoding)
                };

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
                UpdateGeometry(geometry, buildingExtractItemV2);

                await context
                    .BuildingExtractV2
                    .AddAsync(buildingExtractItemV2, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingExtractItemV2 = new BuildingExtractItemV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    DbaseRecord = new BuildingDbaseRecord
                    {
                        id =
                        {
                            Value =
                                $"{extractConfigValue.DataVlaanderenNamespaceBuilding}/{message.Message.BuildingPersistentLocalId}"
                        },
                        gebouwid = {Value = message.Message.BuildingPersistentLocalId},
                        geommet = {Value = MapGeometryMethod(BuildingGeometryMethod.Outlined)},
                        status = {Value = MapStatus(BuildingStatus.Planned)},
                        versieid =
                        {
                            Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset()
                        }
                    }.ToBytes(_encoding)
                };

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()) as Polygon;
                UpdateGeometry(geometry, buildingExtractItemV2);

                await context
                    .BuildingExtractV2
                    .AddAsync(buildingExtractItemV2, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuilding.ToByteArray()) as Polygon;
                UpdateGeometry(geometry, item);

                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.UnderConstruction));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.Planned));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.Realized));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(
                    message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.UnderConstruction));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.NotRealized));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateStatus(item, MapStatus(BuildingStatus.Planned));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                UpdateRecord(item, record => record.IsDeleted = true);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingExtractV2.FindAsync(message.Message.BuildingPersistentLocalId,
                    cancellationToken: ct);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });
        }

        private static string MapGeometryMethod(BuildingGeometryMethod buildingGeometryMethod)
        {
            var dictionary = new Dictionary<BuildingGeometryMethod, string>
            {
                { BuildingGeometryMethod.MeasuredByGrb, MeasuredByGrb },
                { BuildingGeometryMethod.Outlined, Outlined }
            };

            return dictionary[buildingGeometryMethod];
        }

        private static string MapStatus(BuildingStatus buildingStatus)
        {
            var dictionary = new Dictionary<BuildingStatus, string>
            {
                { BuildingStatus.Planned, Planned },
                { BuildingStatus.UnderConstruction, UnderConstruction },
                { BuildingStatus.Realized, Realized },
                { BuildingStatus.NotRealized, NotRealized },
                { BuildingStatus.Retired, Retired }
            };

            return dictionary[buildingStatus];
        }

        private static void UpdateGeometry(Polygon? geometry, BuildingExtractItemV2 item)
        {
            if (geometry == null)
            {
                item.ShapeRecordContentLength = 0;
            }
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

        private void UpdateStatus(BuildingExtractItemV2 building, string status)
            => UpdateRecord(building, record => record.status.Value = status);

        private void UpdateVersie(BuildingExtractItemV2 building, Instant timestamp)
            => UpdateRecord(building, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(BuildingExtractItemV2 building, Action<BuildingDbaseRecord> updateFunc)
        {
            var record = new BuildingDbaseRecord();
            record.FromBytes(building.DbaseRecord, _encoding);

            updateFunc(record);

            building.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
