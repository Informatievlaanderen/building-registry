namespace BuildingRegistry.Projections.Legacy.BuildingDetail
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Events;
    using BuildingRegistry.Legacy.Events.Crab;
    using NodaTime;

    [ConnectedProjectionName("API endpoint detail/lijst gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen detail & lijst voorziet.")]
    public class BuildingDetailProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingDetailProjections()
        {
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

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.PersistentLocalId = message.Message.PersistentLocalId;
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
                SetGeometry(item, message.Message.ExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometry, BuildingGeometryMethod.Outlined);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetails.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometry, BuildingGeometryMethod.Outlined);
                SetVersion(item, message.Message.Provenance.Timestamp);
            });

            #endregion

            //BuildingUnit
            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) => await DoNothing());
            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) => await DoNothing());

            //CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        private static void SetVersion(BuildingDetailItem item, Instant timestamp)
        {
            item.Version = timestamp;
        }

        private void SetGeometry(BuildingDetailItem item, string extendedWkb, BuildingGeometryMethod method)
        {
            item.Geometry = extendedWkb.ToByteArray();
            item.GeometryMethod = method;
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
