namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
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
        private const string Residential = "Wonen";
        private const string Lodging = "Verblijfsrecreatie";
        private const string DayRecreation = "DagrecreatieSport";
        private const string AgricultureHorticulture = "LandTuinbouw";
        private const string Retail = "Detailhandel";
        private const string DancingRestaurantCafe = "DancingRestaurantCafe";
        private const string OfficeServicesLiberalProfession = "KantoorDienstverleningVrijBeroep";
        private const string IndustryBusiness = "IndustrieBedrijvigheid";
        private const string CommunityPublicUtility = "GemeenschapsOpenbareNutsvoorziening";
        private const string MilitaryFunction = "MilitaireFunctie";

        private const string DerivedFromObject = "AfgeleidVanObject";
        private const string AppointedByAdministrator = "AangeduidDoorBeheerder";

        private readonly Encoding _encoding;

        public BuildingUnitExtractV2Projections(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            #region Building
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingUnitBuildingItemV2 = new BuildingUnitBuildingItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    IsRemoved = message.Message.IsRemoved
                };

                var buildingStatus = BuildingStatus.Parse(message.Message.BuildingStatus);
                if (buildingStatus == BuildingStatus.Retired || buildingStatus == BuildingStatus.NotRealized)
                {
                    buildingUnitBuildingItemV2.BuildingRetiredStatus = buildingStatus;
                }

                await context.BuildingUnitBuildingsV2.AddAsync(buildingUnitBuildingItemV2, ct);

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

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitBuildingItemV2 = new BuildingUnitBuildingItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    IsRemoved = false,
                    BuildingRetiredStatus = null
                };

                await context.BuildingUnitBuildingsV2.AddAsync(buildingUnitBuildingItemV2, ct);
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

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var building = await context.BuildingUnitBuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
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
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.IsDeleted = true);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.IsDeleted = true);
                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateStatus(itemV2, MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus)));
                        UpdateRecord(itemV2, record =>
                        {
                            record.functie.Value = MapFunction(BuildingUnitFunction.Parse(message.Message.Function));
                            record.afwijking.Value = message.Message.HasDeviation;
                            record.IsDeleted = false;
                        });

                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                        var geometryMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                        UpdateGeometry(itemV2, geometry);
                        UpdatePosition(itemV2, geometryMethod);

                        UpdateVersie(itemV2, message.Message.Provenance.Timestamp);

                    }, ct);
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

            When<Envelope<BuildingUnitFunctionWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitPersistentLocalId,
                    itemV2 =>
                    {
                        UpdateRecord(itemV2, record => record.functie.Value = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)));
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
        }

        private static string MapFunction(BuildingUnitFunction function)
        {
            var dictionary = new Dictionary<BuildingUnitFunction, string>
            {
                { BuildingUnitFunction.Common, Common },
                { BuildingUnitFunction.Unknown, Unknown },
                { BuildingUnitFunction.Residential, Residential },
                { BuildingUnitFunction.Lodging, Lodging },
                { BuildingUnitFunction.DayRecreationSport, DayRecreation },
                { BuildingUnitFunction.AgricultureHorticulture, AgricultureHorticulture },
                { BuildingUnitFunction.Retail, Retail },
                { BuildingUnitFunction.DancingRestaurantCafe, DancingRestaurantCafe },
                { BuildingUnitFunction.OfficeServicesLiberalProfession, OfficeServicesLiberalProfession },
                { BuildingUnitFunction.IndustryBusiness, IndustryBusiness },
                { BuildingUnitFunction.CommunityPublicUtility, CommunityPublicUtility },
                { BuildingUnitFunction.MilitaryFunction, MilitaryFunction }
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
