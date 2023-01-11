namespace BuildingRegistry.Producer.Extensions
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Events;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.BuildingRegistry;
    using Legacy = Legacy.Events;

    public static class MessageExtensions
    {
        #region Legacy
        public static Contracts.BuildingBecameComplete ToContract(this Legacy.BuildingBecameComplete message) =>
            new Contracts.BuildingBecameComplete(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingBecameIncomplete ToContract(this Legacy.BuildingBecameIncomplete message) =>
            new Contracts.BuildingBecameIncomplete(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingBecameUnderConstruction ToContract(
            this Legacy.BuildingBecameUnderConstruction message) =>
            new Contracts.BuildingBecameUnderConstruction(message.BuildingId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingGeometryWasRemoved ToContract(this Legacy.BuildingGeometryWasRemoved message) =>
            new Contracts.BuildingGeometryWasRemoved(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingMeasurementByGrbWasCorrected ToContract(
            this Legacy.BuildingMeasurementByGrbWasCorrected message) =>
            new Contracts.BuildingMeasurementByGrbWasCorrected(message.BuildingId.ToString("D"),
                message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.BuildingOutlineWasCorrected
            ToContract(this Legacy.BuildingOutlineWasCorrected message) =>
            new Contracts.BuildingOutlineWasCorrected(message.BuildingId.ToString("D"), message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.BuildingPersistentLocalIdWasAssigned ToContract(
            this Legacy.BuildingPersistentLocalIdWasAssigned message) =>
            new Contracts.BuildingPersistentLocalIdWasAssigned(message.BuildingId.ToString("D"),
                message.PersistentLocalId, message.AssignmentDate.ToString(), message.Provenance.ToContract());

        public static Contracts.BuildingStatusWasCorrectedToRemoved ToContract(
            this Legacy.BuildingStatusWasCorrectedToRemoved message) =>
            new Contracts.BuildingStatusWasCorrectedToRemoved(message.BuildingId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingStatusWasRemoved ToContract(this Legacy.BuildingStatusWasRemoved message) =>
            new Contracts.BuildingStatusWasRemoved(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasAttached ToContract(
            this Legacy.BuildingUnitAddressWasAttached message) =>
            new Contracts.BuildingUnitAddressWasAttached(message.BuildingId.ToString("D"),
                message.AddressId.ToString("D"), message.To.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasDetached ToContract(
            this Legacy.BuildingUnitAddressWasDetached message) =>
            new Contracts.BuildingUnitAddressWasDetached(message.BuildingId.ToString("D"),
                message.AddressIds.Select(x => x.ToString("D")), message.From.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitBecameComplete ToContract(this Legacy.BuildingUnitBecameComplete message) =>
            new Contracts.BuildingUnitBecameComplete(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitBecameIncomplete ToContract(
            this Legacy.BuildingUnitBecameIncomplete message) =>
            new Contracts.BuildingUnitBecameIncomplete(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitPersistentLocalIdWasAssigned ToContract(
            this Legacy.BuildingUnitPersistentLocalIdWasAssigned message) =>
            new Contracts.BuildingUnitPersistentLocalIdWasAssigned(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.PersistentLocalId, message.AssignmentDate.ToString(),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitPersistentLocalIdWasDuplicated ToContract(
            this Legacy.BuildingUnitPersistentLocalIdWasDuplicated message) =>
            new Contracts.BuildingUnitPersistentLocalIdWasDuplicated(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.DuplicatePersistentLocalId,
                message.OriginalPersistentLocalId,
                message.DuplicateAssignmentDate.ToString(),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitPersistentLocalIdWasRemoved ToContract(
            this Legacy.BuildingUnitPersistentLocalIdWasRemoved message) =>
            new Contracts.BuildingUnitPersistentLocalIdWasRemoved(message.BuildingId.ToString("D"),
                message.PersistentLocalId, message.AssignmentDate.ToString(), message.Reason,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitPositionWasAppointedByAdministrator ToContract(
            this Legacy.BuildingUnitPositionWasAppointedByAdministrator message) =>
            new Contracts.BuildingUnitPositionWasAppointedByAdministrator(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.BuildingUnitPositionWasCorrectedToAppointedByAdministrator ToContract(
            this Legacy.BuildingUnitPositionWasCorrectedToAppointedByAdministrator message) =>
            new Contracts.BuildingUnitPositionWasCorrectedToAppointedByAdministrator(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.BuildingUnitPositionWasCorrectedToDerivedFromObject ToContract(
            this Legacy.BuildingUnitPositionWasCorrectedToDerivedFromObject message) =>
            new Contracts.BuildingUnitPositionWasCorrectedToDerivedFromObject(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.BuildingUnitPositionWasDerivedFromObject ToContract(
            this Legacy.BuildingUnitPositionWasDerivedFromObject message) =>
            new Contracts.BuildingUnitPositionWasDerivedFromObject(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.BuildingUnitStatusWasRemoved ToContract(
            this Legacy.BuildingUnitStatusWasRemoved message) =>
            new Contracts.BuildingUnitStatusWasRemoved(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasAdded ToContract(this Legacy.BuildingUnitWasAdded message) =>
            new Contracts.BuildingUnitWasAdded(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.BuildingUnitKey.ToString(),
                message.AddressId.ToString("D"),
                message.BuildingUnitVersion.ToString(),
                message.PredecessorBuildingUnitId?.ToString("D") ?? null,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasAddedToRetiredBuilding ToContract(
            this Legacy.BuildingUnitWasAddedToRetiredBuilding message) =>
            new Contracts.BuildingUnitWasAddedToRetiredBuilding(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.BuildingUnitKey.ToString(),
                message.AddressId.ToString("D"),
                message.BuildingUnitVersion.ToString(),
                message.PredecessorBuildingUnitId?.ToString("D") ?? null,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedToNotRealized ToContract(
            this Legacy.BuildingUnitWasCorrectedToNotRealized message) =>
            new Contracts.BuildingUnitWasCorrectedToNotRealized(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedToPlanned ToContract(
            this Legacy.BuildingUnitWasCorrectedToPlanned message) =>
            new Contracts.BuildingUnitWasCorrectedToPlanned(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedToRealized ToContract(
            this Legacy.BuildingUnitWasCorrectedToRealized message) =>
            new Contracts.BuildingUnitWasCorrectedToRealized(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedToRetired ToContract(
            this Legacy.BuildingUnitWasCorrectedToRetired message) =>
            new Contracts.BuildingUnitWasCorrectedToRetired(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasNotRealized ToContract(this Legacy.BuildingUnitWasNotRealized message) =>
            new Contracts.BuildingUnitWasNotRealized(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasNotRealizedByParent ToContract(
            this Legacy.BuildingUnitWasNotRealizedByParent message) =>
            new Contracts.BuildingUnitWasNotRealizedByParent(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ParentBuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasPlanned ToContract(this Legacy.BuildingUnitWasPlanned message) =>
            new Contracts.BuildingUnitWasPlanned(message.BuildingId.ToString("D"), message.BuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasReaddedByOtherUnitRemoval ToContract(
            this Legacy.BuildingUnitWasReaddedByOtherUnitRemoval message) =>
            new Contracts.BuildingUnitWasReaddedByOtherUnitRemoval(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.BuildingUnitKey.ToString(),
                message.AddressId.ToString("D"),
                message.BuildingUnitVersion.ToString(),
                message.PredecessorBuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasReaddressed ToContract(this Legacy.BuildingUnitWasReaddressed message) =>
            new Contracts.BuildingUnitWasReaddressed(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.OldAddressId.ToString("D"),
                message.NewAddressId.ToString("D"),
                message.BeginDate.ToString(),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRealized ToContract(this Legacy.BuildingUnitWasRealized message) =>
            new Contracts.BuildingUnitWasRealized(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRemoved ToContract(this Legacy.BuildingUnitWasRemoved message) =>
            new Contracts.BuildingUnitWasRemoved(message.BuildingId.ToString("D"), message.BuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRetired ToContract(this Legacy.BuildingUnitWasRetired message) =>
            new Contracts.BuildingUnitWasRetired(message.BuildingId.ToString("D"), message.BuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRetiredByParent ToContract(
            this Legacy.BuildingUnitWasRetiredByParent message) =>
            new Contracts.BuildingUnitWasRetiredByParent(message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"), message.ParentBuildingUnitId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToNotRealized ToContract(
            this Legacy.BuildingWasCorrectedToNotRealized message) =>
            new Contracts.BuildingWasCorrectedToNotRealized(
                message.BuildingId.ToString("D"),
                message.BuildingUnitIdsToRetire.Select(x => x.ToString("D")),
                message.BuildingUnitIdsToNotRealize.Select(x => x.ToString("D")),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToPlanned ToContract(
            this Legacy.BuildingWasCorrectedToPlanned message) =>
            new Contracts.BuildingWasCorrectedToPlanned(message.BuildingId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToRealized ToContract(
            this Legacy.BuildingWasCorrectedToRealized message) =>
            new Contracts.BuildingWasCorrectedToRealized(message.BuildingId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToRetired ToContract(
            this Legacy.BuildingWasCorrectedToRetired message) =>
            new Contracts.BuildingWasCorrectedToRetired(
                message.BuildingId.ToString("D"),
                message.BuildingUnitIdsToRetire.Select(x => x.ToString("D")),
                message.BuildingUnitIdsToNotRealize.Select(x => x.ToString("D")),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToUnderConstruction ToContract(
            this Legacy.BuildingWasCorrectedToUnderConstruction message) =>
            new Contracts.BuildingWasCorrectedToUnderConstruction(message.BuildingId.ToString("D"),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasMarkedAsMigrated ToContract(this Legacy.BuildingWasMarkedAsMigrated message) =>
            new Contracts.BuildingWasMarkedAsMigrated(message.BuildingId.ToString("D"), message.PersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasMeasuredByGrb ToContract(this Legacy.BuildingWasMeasuredByGrb message) =>
            new Contracts.BuildingWasMeasuredByGrb(message.BuildingId.ToString("D"), message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasNotRealized ToContract(this Legacy.BuildingWasNotRealized message) =>
            new Contracts.BuildingWasNotRealized(
                message.BuildingId.ToString("D"),
                message.BuildingUnitIdsToRetire.Select(x => x.ToString("D")),
                message.BuildingUnitIdsToNotRealize.Select(x => x.ToString("D")),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasOutlined ToContract(this Legacy.BuildingWasOutlined message) =>
            new Contracts.BuildingWasOutlined(message.BuildingId.ToString("D"), message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasPlanned ToContract(this Legacy.BuildingWasPlanned message) =>
            new Contracts.BuildingWasPlanned(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRealized ToContract(this Legacy.BuildingWasRealized message) =>
            new Contracts.BuildingWasRealized(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRegistered ToContract(this Legacy.BuildingWasRegistered message) =>
            new Contracts.BuildingWasRegistered(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRemoved ToContract(this Legacy.BuildingWasRemoved message) =>
            new Contracts.BuildingWasRemoved(message.BuildingId.ToString("D"), message.BuildingUnitIds.Select(x => x.ToString("D")),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasRetired ToContract(this Legacy.BuildingWasRetired message) =>
            new Contracts.BuildingWasRetired(
                message.BuildingId.ToString("D"),
                message.BuildingUnitIdsToRetire.Select(x => x.ToString("D")),
                message.BuildingUnitIdsToNotRealize.Select(x => x.ToString("D")),
                message.Provenance.ToContract());

        public static Contracts.CommonBuildingUnitWasAdded ToContract(this Legacy.CommonBuildingUnitWasAdded message) =>
            new Contracts.CommonBuildingUnitWasAdded(
                message.BuildingId.ToString("D"),
                message.BuildingUnitId.ToString("D"),
                message.BuildingUnitKey.ToString(),
                message.BuildingUnitVersion.ToString(),
                message.Provenance.ToContract());

        #endregion

        public static Contracts.BuildingBecameUnderConstructionV2 ToContract(this BuildingBecameUnderConstructionV2 message)
            => new Contracts.BuildingBecameUnderConstructionV2(message.BuildingPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingOutlineWasChanged ToContract(this BuildingOutlineWasChanged message)
            => new Contracts.BuildingOutlineWasChanged(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalIds,
                message.ExtendedWkbGeometryBuilding,
                message.ExtendedWkbGeometryBuildingUnits,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasAttachedV2 ToContract(this BuildingUnitAddressWasAttachedV2 message)
            => new Contracts.BuildingUnitAddressWasAttachedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.AddressPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasDetachedV2 ToContract(this BuildingUnitAddressWasDetachedV2 message)
            => new Contracts.BuildingUnitAddressWasDetachedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.AddressPersistentLocalId,
                message.Provenance.ToContract());
        public static Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRemoved ToContract(this BuildingUnitAddressWasDetachedBecauseAddressWasRemoved message)
            => new Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRemoved(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.AddressPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRejected ToContract(this BuildingUnitAddressWasDetachedBecauseAddressWasRejected message)
            => new Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRejected(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.AddressPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRetired ToContract(this BuildingUnitAddressWasDetachedBecauseAddressWasRetired message)
            => new Contracts.BuildingUnitAddressWasDetachedBecauseAddressWasRetired(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.AddressPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitPositionWasCorrected ToContract(this BuildingUnitPositionWasCorrected message)
            => new Contracts.BuildingUnitPositionWasCorrected(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.GeometryMethod,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitRemovalWasCorrected ToContract(this BuildingUnitRemovalWasCorrected message)
            => new Contracts.BuildingUnitRemovalWasCorrected(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.BuildingUnitStatus,
                message.Function,
                message.GeometryMethod,
                message.ExtendedWkbGeometry,
                message.HasDeviation,
                message.AddressPersistentLocalIds,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedFromNotRealizedToPlanned ToContract(this BuildingUnitWasCorrectedFromNotRealizedToPlanned message)
            => new Contracts.BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedFromRealizedToPlanned ToContract(this BuildingUnitWasCorrectedFromRealizedToPlanned message)
            => new Contracts.BuildingUnitWasCorrectedFromRealizedToPlanned(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected ToContract(this BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected message)
            => new Contracts.BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasCorrectedFromRetiredToRealized ToContract(this BuildingUnitWasCorrectedFromRetiredToRealized message)
            => new Contracts.BuildingUnitWasCorrectedFromRetiredToRealized(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasDeregulated ToContract(this BuildingUnitWasDeregulated message)
            => new Contracts.BuildingUnitWasDeregulated(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized ToContract(this BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized message)
            => new Contracts.BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasNotRealizedV2 ToContract(this BuildingUnitWasNotRealizedV2 message)
            => new Contracts.BuildingUnitWasNotRealizedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasPlannedV2 ToContract(this BuildingUnitWasPlannedV2 message)
            => new Contracts.BuildingUnitWasPlannedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.GeometryMethod,
                message.ExtendedWkbGeometry,
                message.Function,
                message.HasDeviation,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRealizedBecauseBuildingWasRealized ToContract(this BuildingUnitWasRealizedBecauseBuildingWasRealized message)
            => new Contracts.BuildingUnitWasRealizedBecauseBuildingWasRealized(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRealizedV2 ToContract(this BuildingUnitWasRealizedV2 message)
            => new Contracts.BuildingUnitWasRealizedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRegularized ToContract(this BuildingUnitWasRegularized message)
            => new Contracts.BuildingUnitWasRegularized(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitRegularizationWasCorrected ToContract(this BuildingUnitRegularizationWasCorrected message)
            => new Contracts.BuildingUnitRegularizationWasCorrected(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRemovedV2 ToContract(this BuildingUnitWasRemovedV2 message)
            => new Contracts.BuildingUnitWasRemovedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRemovedBecauseBuildingWasRemoved ToContract(this BuildingUnitWasRemovedBecauseBuildingWasRemoved message)
            => new Contracts.BuildingUnitWasRemovedBecauseBuildingWasRemoved(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingUnitWasRetiredV2 ToContract(this BuildingUnitWasRetiredV2 message)
            => new Contracts.BuildingUnitWasRetiredV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedFromNotRealizedToPlanned ToContract(this BuildingWasCorrectedFromNotRealizedToPlanned message)
            => new Contracts.BuildingWasCorrectedFromNotRealizedToPlanned(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedFromRealizedToUnderConstruction ToContract(this BuildingWasCorrectedFromRealizedToUnderConstruction message)
            => new Contracts.BuildingWasCorrectedFromRealizedToUnderConstruction(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedFromUnderConstructionToPlanned ToContract(this BuildingWasCorrectedFromUnderConstructionToPlanned message)
            => new Contracts.BuildingWasCorrectedFromUnderConstructionToPlanned(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasMigrated ToContract(this BuildingWasMigrated message)
            => new Contracts.BuildingWasMigrated(
                message.BuildingId.ToString("D"),
                message.BuildingPersistentLocalId,
                message.BuildingPersistentLocalIdAssignmentDate.ToString(),
                message.BuildingStatus,
                message.GeometryMethod,
                message.ExtendedWkbGeometry,
                message.IsRemoved,
                message.BuildingUnits.Select(x =>
                    new Contracts.BuildingWasMigrated.BuildingUnit(
                        x.BuildingUnitId.ToString("D"),
                        x.BuildingUnitPersistentLocalId,
                        x.Function,
                        x.Status,
                        x.AddressPersistentLocalIds,
                        x.GeometryMethod,
                        x.ExtendedWkbGeometry,
                        x.IsRemoved)),
                message.Provenance.ToContract());

        public static Contracts.BuildingWasNotRealizedV2 ToContract(this BuildingWasNotRealizedV2 message)
            => new Contracts.BuildingWasNotRealizedV2(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasPlannedV2 ToContract(this BuildingWasPlannedV2 message)
            => new Contracts.BuildingWasPlannedV2(
                message.BuildingPersistentLocalId,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasRealizedV2 ToContract(this BuildingWasRealizedV2 message)
            => new Contracts.BuildingWasRealizedV2(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.CommonBuildingUnitWasAddedV2 ToContract(this CommonBuildingUnitWasAddedV2 message)
            => new Contracts.CommonBuildingUnitWasAddedV2(
                message.BuildingPersistentLocalId,
                message.BuildingUnitPersistentLocalId,
                message.BuildingUnitStatus,
                message.GeometryMethod,
                message.ExtendedWkbGeometry,
                message.HasDeviation,
                message.Provenance.ToContract());

        public static Contracts.BuildingWasRemovedV2 ToContract(this BuildingWasRemovedV2 message)
            => new Contracts.BuildingWasRemovedV2(
                message.BuildingPersistentLocalId,
                message.Provenance.ToContract());

        private static Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance ToContract(this ProvenanceData provenance)
        => new Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance(
            provenance.Timestamp.ToString(),
            provenance.Application.ToString(),
            provenance.Modification.ToString(),
            provenance.Organisation.ToString(),
            provenance.Reason);
    }
}
