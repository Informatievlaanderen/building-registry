namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using ValueObjects;

    public partial class Building
    {
        private BuildingId _buildingId;
        public BuildingGeometry Geometry { get; private set; }

        private PersistentLocalId _persistentLocalId;
        private BuildingStatus? _status;

        public bool IsRemoved { get; private set; }
        private bool _isComplete;
        private bool IsRetired => _status == BuildingStatus.Retired || _status == BuildingStatus.NotRealized;
        public Modification LastModificationBasedOnCrab { get; private set; }

        private readonly Chronicle<BuildingGeometryWasImportedFromCrab, int> _geometryChronicle = new Chronicle<BuildingGeometryWasImportedFromCrab, int>();
        private readonly Chronicle<BuildingStatusWasImportedFromCrab, int> _statusChronicle = new Chronicle<BuildingStatusWasImportedFromCrab, int>();

        private readonly BuildingUnitCollection _buildingUnitCollection = new BuildingUnitCollection();

        private readonly Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
            _activeHouseNumberIdsByTerreinObjectHouseNr = new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>();

        private readonly Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>> _legacySubaddressEventsByTerreinObjectHouseNumber
            = new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>();

        private readonly Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>> _legacySubaddressStatusEventsBySubadresId
            = new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>();
        private readonly Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>> _legacySubaddressPositionEventsBySubadresId
            = new Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>>();

        private readonly Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>> _legacyHouseNumberStatusEventsByHouseNumberId
            = new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>();
        private readonly Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>> _legacyHouseNumberPositionEventsByHouseNumberId
            = new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>();

        private readonly Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab> _readdressedHouseNumbers = new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>();
        private readonly Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab> _readdressedSubaddresses = new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>();
        private readonly List<CrabTerrainObjectHouseNumberId> _importedTerrainObjectHouseNumberIds = new List<CrabTerrainObjectHouseNumberId>();

        internal Building(ISnapshotStrategy snapshotStrategy)
        {
            Strategy = snapshotStrategy;
        }

        private Building()
        {
            Register<BuildingWasRegistered>(When);
            Register<BuildingWasRemoved>(When);
            Register<BuildingPersistentLocalIdWasAssigned>(When);

            Register<BuildingGeometryWasImportedFromCrab>(When);
            Register<BuildingStatusWasImportedFromCrab>(When);
            Register<TerrainObjectHouseNumberWasImportedFromCrab>(When);
            Register<AddressSubaddressWasImportedFromCrab>(When);
            Register<AddressHouseNumberPositionWasImportedFromCrab>(When);
            Register<AddressSubaddressPositionWasImportedFromCrab>(When);
            Register<AddressHouseNumberStatusWasImportedFromCrab>(When);
            Register<AddressSubaddressStatusWasImportedFromCrab>(When);
            Register<HouseNumberWasReaddressedFromCrab>(When);
            Register<SubaddressWasReaddressedFromCrab>(When);
            Register<AddressHouseNumberWasImportedFromCrab>(@event => WhenCrabEventApplied());
            Register<TerrainObjectWasImportedFromCrab>(@event => WhenCrabEventApplied(@event.Modification == CrabModification.Delete));

            Register<BuildingGeometryWasRemoved>(When);
            Register<BuildingWasMeasuredByGrb>(When);
            Register<BuildingWasOutlined>(When);
            Register<BuildingOutlineWasCorrected>(When);
            Register<BuildingMeasurementByGrbWasCorrected>(When);

            Register<BuildingBecameComplete>(When);
            Register<BuildingBecameIncomplete>(When);

            Register<BuildingBecameUnderConstruction>(When);
            Register<BuildingWasCorrectedToUnderConstruction>(When);
            Register<BuildingWasRetired>(When);
            Register<BuildingWasCorrectedToRetired>(When);
            Register<BuildingWasRealized>(When);
            Register<BuildingWasCorrectedToRealized>(When);
            Register<BuildingWasNotRealized>(When);
            Register<BuildingWasCorrectedToNotRealized>(When);
            Register<BuildingWasPlanned>(When);
            Register<BuildingWasCorrectedToPlanned>(When);
            Register<BuildingStatusWasRemoved>(When);
            Register<BuildingStatusWasCorrectedToRemoved>(When);

            Register<BuildingUnitWasAdded>(When);
            Register<BuildingUnitWasAddedToRetiredBuilding>(When);
            Register<BuildingUnitWasReaddedByOtherUnitRemoval>(When);
            Register<CommonBuildingUnitWasAdded>(When);
            Register<BuildingUnitWasRemoved>(When);
            Register<BuildingUnitBecameComplete>(When);
            Register<BuildingUnitBecameIncomplete>(When);

            Register<BuildingUnitWasPlanned>(When);
            Register<BuildingUnitWasCorrectedToPlanned>(When);
            Register<BuildingUnitWasRetired>(When);
            Register<BuildingUnitWasRetiredByParent>(When);
            Register<BuildingUnitWasCorrectedToRetired>(When);
            Register<BuildingUnitWasNotRealized>(When);
            Register<BuildingUnitWasNotRealizedByParent>(When);
            Register<BuildingUnitWasCorrectedToNotRealized>(When);
            Register<BuildingUnitWasRealized>(When);
            Register<BuildingUnitWasCorrectedToRealized>(When);
            Register<BuildingUnitStatusWasRemoved>(When);

            Register<BuildingUnitPositionWasAppointedByAdministrator>(When);
            Register<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>(When);
            Register<BuildingUnitPositionWasDerivedFromObject>(When);
            Register<BuildingUnitPositionWasCorrectedToDerivedFromObject>(When);

            Register<BuildingUnitAddressWasDetached>(When);
            Register<BuildingUnitAddressWasAttached>(When);
            Register<BuildingUnitWasReaddressed>(When);
            Register<BuildingUnitPersistentLocalIdWasAssigned>(When);
        }

        private void When(BuildingUnitWasReaddressed @event)
        {
            var buildingUnit = _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId));
            buildingUnit.Route(@event);
        }

        private void When(BuildingPersistentLocalIdWasAssigned @event)
        {
            _persistentLocalId = new PersistentLocalId(@event.PersistentLocalId);
        }

        private void When(BuildingUnitPersistentLocalIdWasAssigned @event)
        {
            var unit = _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId));
            unit.Route(@event);
        }

        private void When(BuildingUnitAddressWasAttached @event)
        {
            var to = _buildingUnitCollection.GetById(new BuildingUnitId(@event.To));
            to.Route(@event);
        }

        private void When(BuildingUnitAddressWasDetached @event)
        {
            var from = _buildingUnitCollection.GetById(new BuildingUnitId(@event.From));
            from.Route(@event);
        }

        #region BuildingUnitStatus

        private void When(BuildingUnitBecameComplete @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitBecameIncomplete @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasCorrectedToNotRealized @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasCorrectedToRetired @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasCorrectedToPlanned @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasCorrectedToRealized @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasRealized @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasNotRealized @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasRetired @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasPlanned @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitStatusWasRemoved @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasRetiredByParent @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasNotRealizedByParent @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        #endregion BuildingUnit Status

        #region BuildingUnitPosition
        private void When(BuildingUnitPositionWasCorrectedToDerivedFromObject @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitPositionWasDerivedFromObject @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitPositionWasCorrectedToAppointedByAdministrator @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitPositionWasAppointedByAdministrator @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }
        #endregion BuildingUnitPosition

        private void When(BuildingUnitWasRemoved @event)
        {
            _buildingUnitCollection.GetById(new BuildingUnitId(@event.BuildingUnitId)).Route(@event);
        }

        private void When(BuildingUnitWasAdded @event)
        {
            AddBuildingUnit(@event);
        }

        private void When(BuildingUnitWasAddedToRetiredBuilding @event)
        {
            AddBuildingUnit(@event);
        }

        private void When(CommonBuildingUnitWasAdded @event)
        {
            AddBuildingUnit(@event);
        }

        private void When(BuildingUnitWasReaddedByOtherUnitRemoval @event)
        {
            AddBuildingUnit(@event);
        }

        private void AddBuildingUnit(object @event)
        {
            var buildingUnit = new BuildingUnit(ApplyChange, this);
            buildingUnit.Route(@event);

            if (buildingUnit.BuildingUnitKey.Subaddress.HasValue)
            {
                var crabSubaddressId = new CrabSubaddressId(buildingUnit.BuildingUnitKey.Subaddress.Value);
                if (_legacySubaddressPositionEventsBySubadresId.ContainsKey(crabSubaddressId))
                {
                    foreach (var subaddressPositionWasImportedFromCrab in _legacySubaddressPositionEventsBySubadresId[crabSubaddressId])
                        buildingUnit.Route(subaddressPositionWasImportedFromCrab);
                }

                if (_legacySubaddressStatusEventsBySubadresId.ContainsKey(crabSubaddressId))
                {
                    foreach (var subaddressStatusWasImportedFromCrab in _legacySubaddressStatusEventsBySubadresId[crabSubaddressId])
                        buildingUnit.Route(subaddressStatusWasImportedFromCrab);
                }
            }
            else if(buildingUnit.BuildingUnitKey.HouseNumber.HasValue && buildingUnit.AddressIds.Any())
            {
                if (_legacyHouseNumberPositionEventsByHouseNumberId.ContainsKey(buildingUnit.AddressIds.First()))
                {
                    foreach (var houseNumberPositionWasImportedFromCrab in _legacyHouseNumberPositionEventsByHouseNumberId[buildingUnit.AddressIds.First()])
                        buildingUnit.Route(houseNumberPositionWasImportedFromCrab);
                }

                if (_legacyHouseNumberStatusEventsByHouseNumberId.ContainsKey(buildingUnit.AddressIds.First()))
                {
                    foreach (var houseNumberStatusWasImportedFromCrab in _legacyHouseNumberStatusEventsByHouseNumberId[buildingUnit.AddressIds.First()])
                        buildingUnit.Route(houseNumberStatusWasImportedFromCrab);
                }
            }

            _buildingUnitCollection.Add(buildingUnit);
        }

        #region Building

        private void When(BuildingWasCorrectedToNotRealized @event)
        {
            _status = BuildingStatus.NotRealized;
            WhenBuildingRetired(@event.BuildingUnitIdsToRetire, @event.BuildingUnitIdsToNotRealize, @event);
        }

        private void WhenBuildingRetired(
            IEnumerable<Guid> buildingUnitIdsToRetire,
            IEnumerable<Guid> buildingUnitIdsToNotRealize,
            object @event)
        {
            foreach (var buildingUnitId in buildingUnitIdsToRetire)
            {
                var id = new BuildingUnitId(buildingUnitId);
                _buildingUnitCollection.GetById(id).Route(@event);
            }

            foreach (var buildingUnitId in buildingUnitIdsToNotRealize)
            {
                var id = new BuildingUnitId(buildingUnitId);
                _buildingUnitCollection.GetById(id).Route(@event);
            }
        }

        private void When(BuildingWasCorrectedToPlanned @event)
        {
            _status = BuildingStatus.Planned;
        }

        private void When(BuildingWasCorrectedToRealized @event)
        {
            _status = BuildingStatus.Realized;
        }

        private void When(BuildingWasCorrectedToRetired @event)
        {
            _status = BuildingStatus.Retired;
            WhenBuildingRetired(@event.BuildingUnitIdsToRetire, @event.BuildingUnitIdsToNotRealize, @event);
        }

        private void When(BuildingWasCorrectedToUnderConstruction @event)
        {
            _status = BuildingStatus.UnderConstruction;
        }

        private void When(BuildingStatusWasRemoved @event)
        {
            _status = null;
        }

        private void When(BuildingStatusWasCorrectedToRemoved obj)
        {
            _status = null;
        }

        private void When(BuildingBecameUnderConstruction @event)
        {
            _status = BuildingStatus.UnderConstruction;
        }

        private void When(BuildingWasRetired @event)
        {
            _status = BuildingStatus.Retired;
            WhenBuildingRetired(@event.BuildingUnitIdsToRetire, @event.BuildingUnitIdsToNotRealize, @event);
        }

        private void When(BuildingWasRealized @event)
        {
            _status = BuildingStatus.Realized;
        }

        private void When(BuildingWasNotRealized @event)
        {
            _status = BuildingStatus.NotRealized;
            WhenBuildingRetired(@event.BuildingUnitIdsToRetire, @event.BuildingUnitIdsToNotRealize, @event);
        }

        private void When(BuildingWasPlanned @event)
        {
            _status = BuildingStatus.Planned;
        }

        private void When(BuildingBecameComplete @event)
        {
            _isComplete = true;
        }

        private void When(BuildingBecameIncomplete @event)
        {
            _isComplete = false;
        }

        private void When(BuildingWasOutlined @event)
        {
            Geometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkbGeometry), BuildingGeometryMethod.Outlined);
            _buildingUnitCollection.RouteToNonDeleted(@event);
        }

        private void When(BuildingWasMeasuredByGrb @event)
        {
            Geometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkbGeometry), BuildingGeometryMethod.MeasuredByGrb);
            _buildingUnitCollection.RouteToNonDeleted(@event);
        }

        private void When(BuildingMeasurementByGrbWasCorrected @event)
        {
            Geometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkbGeometry), BuildingGeometryMethod.MeasuredByGrb);
            _buildingUnitCollection.RouteToNonDeleted(@event);
        }

        private void When(BuildingOutlineWasCorrected @event)
        {
            Geometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkbGeometry), BuildingGeometryMethod.Outlined);
            _buildingUnitCollection.RouteToNonDeleted(@event);
        }

        private void When(BuildingGeometryWasRemoved @event)
        {
            Geometry = null;
            _buildingUnitCollection.RouteToNonDeleted(@event);
        }

        private void When(BuildingWasRemoved @event)
        {
            IsRemoved = true;
            foreach (var buildingUnitId in @event.BuildingUnitIds)
            {
                _buildingUnitCollection
                    .GetById(new BuildingUnitId(buildingUnitId))
                    .Route(@event);
            }
        }

        private void When(BuildingWasRegistered @event)
        {
            _buildingId = new BuildingId(@event.BuildingId);
        }

        #endregion Building

        #region CRAB
        private void When(BuildingGeometryWasImportedFromCrab @event)
        {
            _geometryChronicle.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(BuildingStatusWasImportedFromCrab @event)
        {
            _statusChronicle.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(TerrainObjectHouseNumberWasImportedFromCrab @event)
        {
            var crabTerrainObjectHouseNumberId = new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId);

            if(!_importedTerrainObjectHouseNumberIds.Contains(crabTerrainObjectHouseNumberId))
                _importedTerrainObjectHouseNumberIds.Add(crabTerrainObjectHouseNumberId);

            var crabHouseNumberId = new CrabHouseNumberId(@event.HouseNumberId);
            if (@event.Modification == CrabModification.Delete)
                _activeHouseNumberIdsByTerreinObjectHouseNr.Remove(crabTerrainObjectHouseNumberId);
            else
                _activeHouseNumberIdsByTerreinObjectHouseNr[crabTerrainObjectHouseNumberId] = crabHouseNumberId;

            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressPositionWasImportedFromCrab @event)
        {
            var crabSubaddressId = new CrabSubaddressId(@event.SubaddressId);
            var key = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId), crabSubaddressId);

            if (IsSubaddressReaddressedAt(key, new CrabTimestamp(@event.Timestamp)))
                return;

            _buildingUnitCollection.GetActiveOrLastRetiredByKey(key)?.Route(@event);

            if (!_legacySubaddressPositionEventsBySubadresId.ContainsKey(crabSubaddressId))
                _legacySubaddressPositionEventsBySubadresId.Add(crabSubaddressId, new List<AddressSubaddressPositionWasImportedFromCrab>());

            _legacySubaddressPositionEventsBySubadresId[crabSubaddressId].Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressStatusWasImportedFromCrab @event)
        {
            var crabSubaddressId = new CrabSubaddressId(@event.SubaddressId);
            var key = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId), crabSubaddressId);

            if (IsSubaddressReaddressedAt(key, new CrabTimestamp(@event.Timestamp)))
                return;

            _buildingUnitCollection.GetActiveOrLastRetiredByKey(key)?.Route(@event);

            if (!_legacySubaddressStatusEventsBySubadresId.ContainsKey(crabSubaddressId))
                _legacySubaddressStatusEventsBySubadresId.Add(crabSubaddressId, new List<AddressSubaddressStatusWasImportedFromCrab>());

            _legacySubaddressStatusEventsBySubadresId[crabSubaddressId].Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressHouseNumberStatusWasImportedFromCrab @event)
        {
            var key = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId));

            if (IsHouseNumberReaddressedAt(key, new CrabTimestamp(@event.Timestamp)))
                return;

            _buildingUnitCollection.GetActiveOrLastRetiredByKey(key)?.Route(@event);

            var addressId = AddressId.CreateFor(new CrabHouseNumberId(@event.HouseNumberId));
            if (!_legacyHouseNumberStatusEventsByHouseNumberId.ContainsKey(addressId))
                _legacyHouseNumberStatusEventsByHouseNumberId.Add(addressId, new List<AddressHouseNumberStatusWasImportedFromCrab>());

            _legacyHouseNumberStatusEventsByHouseNumberId[addressId].Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressHouseNumberPositionWasImportedFromCrab @event)
        {
            var key = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId));

            if (IsHouseNumberReaddressedAt(key, new CrabTimestamp(@event.Timestamp)))
                return;

            _buildingUnitCollection.GetActiveOrLastRetiredByKey(key)?.Route(@event);

            var addressId = AddressId.CreateFor(new CrabHouseNumberId(@event.HouseNumberId));
            if (!_legacyHouseNumberPositionEventsByHouseNumberId.ContainsKey(addressId))
                _legacyHouseNumberPositionEventsByHouseNumberId.Add(addressId, new List<AddressHouseNumberPositionWasImportedFromCrab>());

            _legacyHouseNumberPositionEventsByHouseNumberId[addressId].Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressWasImportedFromCrab @event)
        {
            var buildingUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId), new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId), new CrabSubaddressId(@event.SubaddressId));
            _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey)?.Route(@event);

            var key = Tuple.Create(new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId), new CrabHouseNumberId(@event.HouseNumberId));
            if (!_legacySubaddressEventsByTerreinObjectHouseNumber.ContainsKey(key))
                _legacySubaddressEventsByTerreinObjectHouseNumber.Add(key, new List<AddressSubaddressWasImportedFromCrab>());

            _legacySubaddressEventsByTerreinObjectHouseNumber[key].Add(@event);
            WhenCrabEventApplied();
        }

        private void When(HouseNumberWasReaddressedFromCrab @event)
        {
            var oldBuildingUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId), new CrabTerrainObjectHouseNumberId(@event.OldTerrainObjectHouseNumberId));
            _buildingUnitCollection.AddReaddress(
                BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId), new CrabTerrainObjectHouseNumberId(@event.NewTerrainObjectHouseNumberId)),
                oldBuildingUnitKey);

            _activeHouseNumberIdsByTerreinObjectHouseNr.Remove(new CrabTerrainObjectHouseNumberId(@event.OldTerrainObjectHouseNumberId));
            _activeHouseNumberIdsByTerreinObjectHouseNr.Add(new CrabTerrainObjectHouseNumberId(@event.NewTerrainObjectHouseNumberId), new CrabHouseNumberId(@event.NewHouseNumberId));

            _readdressedHouseNumbers.Add(oldBuildingUnitKey, @event);
        }

        private void When(SubaddressWasReaddressedFromCrab @event)
        {
            var newBuildingUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId), new CrabTerrainObjectHouseNumberId(@event.NewTerrainObjectHouseNumberId), new CrabSubaddressId(@event.NewSubaddressId));
            var oldBuildingUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(@event.TerrainObjectId), new CrabTerrainObjectHouseNumberId(@event.OldTerrainObjectHouseNumberId), new CrabSubaddressId(@event.OldSubaddressId));
            _buildingUnitCollection.AddReaddress(
                newBuildingUnitKey,
                oldBuildingUnitKey);

            _readdressedSubaddresses.Add(oldBuildingUnitKey, @event);
        }

        private void WhenCrabEventApplied(bool isDeleted = false)
        {
            if (isDeleted)
                LastModificationBasedOnCrab = Modification.Delete;
            else if (LastModificationBasedOnCrab == Modification.Unknown)
                LastModificationBasedOnCrab = Modification.Insert;
            else if (LastModificationBasedOnCrab == Modification.Insert)
                LastModificationBasedOnCrab = Modification.Update;
        }
        #endregion

        private BuildingUnit GetPredecessorFor(BuildingUnitKey buildingUnitKey)
        {
            if (!_buildingUnitCollection.HasRetiredUnitByKey(buildingUnitKey))
                return null;

            return _buildingUnitCollection
                .GetRetiredUnitsByKey(buildingUnitKey)
                .OrderBy(x => x.Version)
                .Last();
        }

        public BuildingUnit GetBuildingUnitById(BuildingUnitId buildingUnitId)
        {
            return _buildingUnitCollection.GetById(buildingUnitId);
        }

        public object TakeSnapshot()
        {
            return new BuildingSnapshot(
                _buildingId,
                _persistentLocalId,
                Geometry,
                _status,
                _isComplete,
                IsRemoved,
                _geometryChronicle.ToList(),
                _statusChronicle.ToList(),
                _activeHouseNumberIdsByTerreinObjectHouseNr,
                _legacySubaddressEventsByTerreinObjectHouseNumber,
                _legacySubaddressStatusEventsBySubadresId,
                _legacySubaddressPositionEventsBySubadresId,
                _legacyHouseNumberStatusEventsByHouseNumberId,
                _legacyHouseNumberPositionEventsByHouseNumberId,
                _readdressedHouseNumbers,
                _readdressedSubaddresses,
                _importedTerrainObjectHouseNumberIds,
                LastModificationBasedOnCrab);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
