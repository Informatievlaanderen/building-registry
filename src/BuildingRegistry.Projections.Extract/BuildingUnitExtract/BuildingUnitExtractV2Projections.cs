namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Point = Be.Vlaanderen.Basisregisters.Shaperon.Point;

    [ConnectedProjectionName("Extract gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het gebouweenheden extract voorziet.")]
    public class BuildingUnitExtractV2Projections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";

        private const string Unknown = "NietGekend";
        private const string Common = "GemeenschappelijkDeel";

        private const string DerivedFromObject = "AfgeleidVanObject";
        private const string AppointedByAdministrator = "AangeduidDoorBeheerder";

        private readonly Encoding _encoding;

        public BuildingUnitExtractV2Projections(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                {
                    return;
                }

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    if (buildingUnit.IsRemoved)
                    {
                        continue;
                    }

                    var buildingUnitItemV2 = new BuildingUnitExtractItemV2
                    {
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            id = { Value = $"{extractConfig.Value.DataVlaanderenNamespaceBuildingUnit}/{buildingUnit.BuildingUnitPersistentLocalId}" },
                            gebouwehid = { Value = buildingUnit.BuildingUnitPersistentLocalId },
                            gebouwid = { Value = message.Message.BuildingPersistentLocalId.ToString() },
                            functie = { Value = MapFunction(BuildingUnitFunction.Parse(buildingUnit.Function)) },
                            status = { Value = MapStatus(BuildingUnitStatus.Parse(buildingUnit.Status)) },
                            afwijking = { Value = false },
                            posgeommet = { Value = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)) },
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                        }.ToBytes(_encoding)
                    };

                    var geometry = wkbReader.Read(buildingUnit.ExtendedWkbGeometry.ToByteArray());
                    UpdateGeometry(buildingUnitItemV2, geometry);

                    await context.BuildingUnitExtractV2.AddAsync(buildingUnitItemV2, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await context.FindAndUpdateBuildingUnitExtract(buildingUnitPersistentLocalId,
                        itemV2 =>
                        {
                            var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                            UpdateGeometry(itemV2, geometry);
                            var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
                            UpdatePosition(itemV2, geometryMethod);
                            UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                        }, ct);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnitExtract(buildingUnitPersistentLocalId,
                        itemV2 =>
                        {
                            var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                            UpdateGeometry(itemV2, geometry);
                            var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
                            UpdatePosition(itemV2, geometryMethod);
                            UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                        }, ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnitExtract(buildingUnitPersistentLocalId,
                        itemV2 =>
                        {
                            var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                            UpdateGeometry(itemV2, geometry);
                            var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
                            UpdatePosition(itemV2, geometryMethod);
                            UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                        }, ct);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnitExtract(buildingUnitPersistentLocalId,
                        itemV2 =>
                        {
                            var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                            UpdateGeometry(itemV2, geometry);
                            var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
                            UpdatePosition(itemV2, geometryMethod);
                            UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                        }, ct);
                }
            });

            #endregion Building

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitItemV2 = new BuildingUnitExtractItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    DbaseRecord = new BuildingUnitDbaseRecord
                    {
                        id = { Value = $"{extractConfig.Value.DataVlaanderenNamespaceBuildingUnit}/{message.Message.BuildingUnitPersistentLocalId}" },
                        gebouwehid = { Value = message.Message.BuildingUnitPersistentLocalId },
                        gebouwid = { Value = message.Message.BuildingPersistentLocalId.ToString() },
                        functie = { Value = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)) },
                        status = { Value = Planned },
                        afwijking = { Value = message.Message.HasDeviation },
                        posgeommet = { Value = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)) },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                UpdateGeometry(buildingUnitItemV2, geometry);

                await context.BuildingUnitExtractV2.AddAsync(buildingUnitItemV2, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Realized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Realized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Planned);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Planned);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, NotRealized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, NotRealized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Planned);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Retired);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Realized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var itemV2 = await context
                    .BuildingUnitExtractV2
                    .FindAsync(message.Message.BuildingUnitPersistentLocalId, cancellationToken: ct);

                context.BuildingUnitExtractV2.Remove(itemV2);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                var itemV2 = await context.BuildingUnitExtractV2
                    .FindAsync(message.Message.BuildingUnitPersistentLocalId, cancellationToken: ct);

                context.BuildingUnitExtractV2.Remove(itemV2);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var buildingUnitItemV2 = new BuildingUnitExtractItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    DbaseRecord = new BuildingUnitDbaseRecord
                    {
                        id = { Value = $"{extractConfig.Value.DataVlaanderenNamespaceBuildingUnit}/{message.Message.BuildingUnitPersistentLocalId}" },
                        gebouwehid = { Value = message.Message.BuildingUnitPersistentLocalId },
                        gebouwid = { Value = message.Message.BuildingPersistentLocalId.ToString() },
                        functie = { Value = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)) },
                        status = { Value = MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus)) },
                        afwijking = { Value = message.Message.HasDeviation },
                        posgeommet = { Value = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)) },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                UpdateGeometry(buildingUnitItemV2, geometry);

                await context.BuildingUnitExtractV2.AddAsync(buildingUnitItemV2, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.afwijking.Value = false);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.afwijking.Value = true);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.afwijking.Value = true);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.afwijking.Value = false);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var commonBuildingUnitItemV2 = new BuildingUnitExtractItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    DbaseRecord = new BuildingUnitDbaseRecord
                    {
                        id = { Value = $"{extractConfig.Value.DataVlaanderenNamespaceBuildingUnit}/{message.Message.BuildingUnitPersistentLocalId}" },
                        gebouwehid = { Value = message.Message.BuildingUnitPersistentLocalId },
                        gebouwid = { Value = message.Message.BuildingPersistentLocalId.ToString() },
                        functie = { Value = MapFunction(BuildingUnitFunction.Common) },
                        status = { Value = MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus)) },
                        afwijking = { Value = message.Message.HasDeviation },
                        posgeommet = { Value = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)) },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };

                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                UpdateGeometry(commonBuildingUnitItemV2, geometry);

                await context.BuildingUnitExtractV2.AddAsync(commonBuildingUnitItemV2, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometryMethod =
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdatePosition(itemV2, geometryMethod);
                        UpdateGeometry(itemV2, geometry);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, Retired);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, NotRealized);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            {
                var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        itemV2.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        UpdateRecord(itemV2, record =>
                            record.id.Value = $"{extractConfig.Value.DataVlaanderenNamespaceBuildingUnit}/{message.Message.BuildingUnitPersistentLocalId}");
                        UpdateRecord(itemV2, record => record.gebouwid.Value = message.Message.BuildingPersistentLocalId.ToString());
                        UpdateRecord(itemV2, record => record.functie.Value = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)));
                        UpdateRecord(itemV2, record => record.status.Value = MapStatus(BuildingUnitStatus.Parse(message.Message.Status)));
                        UpdateRecord(itemV2, record => record.afwijking.Value = message.Message.HasDeviation);
                        UpdatePosition(itemV2, geometryMethod);
                        UpdateGeometry(itemV2, geometry);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasMoved>>(async (context, message, ct) =>
            {
                // Bu state is already updated in BuildingUnitWasTransferred
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });
        }

        private static string MapFunction(BuildingUnitFunction function)
        {
            var dictionary = new Dictionary<BuildingUnitFunction, string>
            {
                { BuildingUnitFunction.Common, Common },
                { BuildingUnitFunction.Unknown, Unknown }
            };

            return dictionary[function];
        }

        private static string MapStatus(BuildingUnitStatus status)
        {
            var dictionary = new Dictionary<BuildingUnitStatus, string>
            {
                { BuildingUnitStatus.Planned, Planned },
                { BuildingUnitStatus.Retired, Retired },
                { BuildingUnitStatus.NotRealized, NotRealized },
                { BuildingUnitStatus.Realized, Realized }
            };

            return dictionary[status];
        }

        private static string MapGeometryMethod(BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var dictionary = new Dictionary<BuildingUnitPositionGeometryMethod, string>
            {
                { BuildingUnitPositionGeometryMethod.AppointedByAdministrator, AppointedByAdministrator },
                { BuildingUnitPositionGeometryMethod.DerivedFromObject, DerivedFromObject }
            };

            return dictionary[geometryMethod];
        }

        private static void UpdateGeometry(BuildingUnitExtractItemV2 item, Geometry? geometry)
        {
            if (geometry == null)
            {
                item.ShapeRecordContentLength = 0;
                item.ShapeRecordContent = null;
                item.MinimumY = 0;
                item.MinimumX = 0;
                item.MaximumY = 0;
                item.MaximumX = 0;
            }
            else
            {
                var pointShapeContent = new PointShapeContent(new Point(geometry.Coordinate.X, geometry.Coordinate.Y));
                item.ShapeRecordContent = pointShapeContent.ToBytes();
                item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
                item.MinimumX = pointShapeContent.Shape.X;
                item.MaximumX = pointShapeContent.Shape.X;
                item.MinimumY = pointShapeContent.Shape.Y;
                item.MaximumY = pointShapeContent.Shape.Y;
            }
        }

        private void UpdateStatus(BuildingUnitExtractItemV2 buildingUnit, string status)
            => UpdateRecord(buildingUnit, record => record.status.Value = status);

        private void UpdateVersie(BuildingUnitExtractItemV2 buildingUnit, Instant timestamp)
            => UpdateRecord(buildingUnit, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(BuildingUnitExtractItemV2 buildingUnit, Action<BuildingUnitDbaseRecord> updateFunc)
        {
            var record = new BuildingUnitDbaseRecord();
            record.FromBytes(buildingUnit.DbaseRecord, _encoding);

            updateFunc(record);

            buildingUnit.DbaseRecord = record.ToBytes(_encoding);
        }

        private void UpdatePosition(BuildingUnitExtractItemV2 buildingUnit, string? posgeommet)
            => UpdateRecord(buildingUnit, record => record.posgeommet.Value = posgeommet);
    }
}
