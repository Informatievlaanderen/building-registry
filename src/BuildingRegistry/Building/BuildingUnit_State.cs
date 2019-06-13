namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events;
    using Events.Crab;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public partial class BuildingUnit
    {
        private readonly Building _parent;

        private Chronicle<AddressHouseNumberStatusWasImportedFromCrab, int> _houseNumberStatusChronicle
            = new Chronicle<AddressHouseNumberStatusWasImportedFromCrab, int>();
        private Chronicle<AddressSubadressStatusWasImportedFromCrab, int> _subaddressStatusChronicle
            = new Chronicle<AddressSubadressStatusWasImportedFromCrab, int>();

        private List<AddressHouseNumberPositionWasImportedFromCrab> _houseNumberPositionsFromCrab
            = new List<AddressHouseNumberPositionWasImportedFromCrab>();

        private List<AddressSubaddressPositionWasImportedFromCrab> _subaddressPositionsFromCrab
            = new List<AddressSubaddressPositionWasImportedFromCrab>();

        private List<BuildingUnitWasReaddressed> _readdressedEvents = new List<BuildingUnitWasReaddressed>();

        private BuildingId _buildingId;

        public BuildingUnitKey BuildingUnitKey { get; private set; }
        public BuildingUnitId BuildingUnitId { get; private set; }
        public BuildingUnitFunction Function { get; private set; }
        public BuildingUnitStatus? Status { get; private set; }
        public List<AddressId> AddressIds { get; private set; }
        public AddressId PreviousAddressId { get; private set; }
        public BuildingGeometry BuildingGeometry { get; internal set; }
        public BuildingUnitPosition BuildingUnitPosition { get; private set; }
        public OsloId OsloId { get; private set; }

        public bool HasRetiredState => Status == BuildingUnitStatus.Retired || Status == BuildingUnitStatus.NotRealized;

        public bool IsRemoved { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsCommon => Function == BuildingUnitFunction.Common;

        public bool IsRetiredByParent { get; private set; }
        public bool IsRetiredByBuilding { get; private set; }
        public bool IsRetiredBySelf { get; private set; }

        public Instant Version { get; private set; }

        public BuildingUnit(Action<object> applier, Building parent) : base(applier)
        {
            _parent = parent;

            //Register<BuildingWasRetired>(When);
            //Register<BuildingWasNotRealized>(When);
            //Register<BuildingWasCorrectedToRetired>(When);
            //Register<BuildingWasCorrectedToNotRealized>(When);
            Register<BuildingUnitOsloIdWasAssigned>(When);

            Register<BuildingWasMeasuredByGrb>(When);
            Register<BuildingWasOutlined>(When);
            Register<BuildingMeasurementByGrbWasCorrected>(When);
            Register<BuildingOutlineWasCorrected>(When);
            Register<BuildingGeometryWasRemoved>(When);
            Register<BuildingWasRemoved>(When);

            Register<BuildingUnitWasAdded>(When);
            Register<BuildingUnitBecameComplete>(When);
            Register<BuildingUnitBecameIncomplete>(When);
            Register<BuildingUnitWasAddedToRetiredBuilding>(When);
            Register<BuildingUnitWasReaddedByOtherUnitRemoval>(When);
            Register<CommonBuildingUnitWasAdded>(When);
            Register<BuildingUnitWasRemoved>(When);
            Register<BuildingUnitStatusWasRemoved>(When);
            Register<BuildingUnitWasPlanned>(When);
            Register<BuildingUnitWasRetired>(When);
            Register<BuildingUnitWasRetiredByParent>(When);
            Register<BuildingUnitWasNotRealized>(When);
            Register<BuildingUnitWasNotRealizedByParent>(When);
            Register<BuildingUnitWasNotRealizedByBuilding>(When);
            Register<BuildingUnitWasRealized>(When);
            Register<BuildingUnitWasCorrectedToRealized>(When);
            Register<BuildingUnitWasCorrectedToPlanned>(When);
            Register<BuildingUnitWasCorrectedToRetired>(When);
            Register<BuildingUnitWasCorrectedToNotRealized>(When);

            Register<BuildingUnitPositionWasAppointedByAdministrator>(When);
            Register<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>(When);
            Register<BuildingUnitPositionWasDerivedFromObject>(When);
            Register<BuildingUnitPositionWasCorrectedToDerivedFromObject>(When);

            Register<BuildingUnitAddressWasDetached>(When);
            Register<BuildingUnitAddressWasAttached>(When);
            Register<BuildingUnitWasReaddressed>(When);

            Register<AddressHouseNumberPositionWasImportedFromCrab>(When);
            Register<AddressSubaddressPositionWasImportedFromCrab>(When);
            Register<AddressHouseNumberStatusWasImportedFromCrab>(When);
            Register<AddressSubadressStatusWasImportedFromCrab>(When);
            Register<AddressSubaddressWasImportedFromCrab>(When);

            AddressIds = new List<AddressId>();
        }

        private void When(BuildingUnitBecameComplete @event)
        {
            IsComplete = true;
        }

        private void When(BuildingUnitBecameIncomplete @event)
        {
            IsComplete = false;
        }

        private void When(BuildingWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(BuildingUnitOsloIdWasAssigned @event)
        {
            OsloId = new OsloId(@event.OsloId);
        }

        #region Unit Position
        private void When(BuildingUnitPositionWasDerivedFromObject @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(new ExtendedWkbGeometry(@event.Position), BuildingUnitPositionGeometryMethod.DerivedFromObject);
        }

        private void When(BuildingUnitPositionWasCorrectedToAppointedByAdministrator @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(new ExtendedWkbGeometry(@event.Position), BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
        }

        private void When(BuildingUnitPositionWasAppointedByAdministrator @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(new ExtendedWkbGeometry(@event.Position), BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
        }

        private void When(BuildingUnitPositionWasCorrectedToDerivedFromObject @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(new ExtendedWkbGeometry(@event.Position), BuildingUnitPositionGeometryMethod.DerivedFromObject);
        }

        #endregion

        #region Building Geometry

        private void When(BuildingGeometryWasRemoved @event)
        {
            BuildingGeometry = null;
            BuildingUnitPosition = null;
        }

        private void When(BuildingOutlineWasCorrected @event)
        {
            BuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkb), BuildingGeometryMethod.Outlined);
        }

        private void When(BuildingMeasurementByGrbWasCorrected @event)
        {
            BuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkb), BuildingGeometryMethod.MeasuredByGrb);
        }

        private void When(BuildingWasOutlined @event)
        {
            BuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkb), BuildingGeometryMethod.Outlined);
        }

        private void When(BuildingWasMeasuredByGrb @event)
        {
            BuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkb), BuildingGeometryMethod.MeasuredByGrb);
        }

        #endregion Building Geometry

        private void When(BuildingUnitAddressWasDetached @event)
        {
            if (@event.AddressIds.Count == 1)
            {
                PreviousAddressId = new AddressId(@event.AddressIds.Single());
                AddressIds.Remove(new AddressId(@event.AddressIds.Single()));
            }
        }

        private void When(BuildingUnitAddressWasAttached @event)
        {
            AddressIds.Add(new AddressId(@event.AddressId));
        }

        private void When(BuildingUnitWasReaddressed @event)
        {
            _readdressedEvents.Add(@event);

            var currentAddressId = new AddressId(@event.OldAddressId);
            var newAddressId = new AddressId(@event.NewAddressId);

            if (PreviousAddressId == currentAddressId)
                PreviousAddressId = newAddressId;

            if (AddressIds.Contains(currentAddressId))
            {
                AddressIds.Remove(currentAddressId);
                AddressIds.Add(newAddressId);
            }
        }

        #region Status

        private void When(BuildingUnitWasCorrectedToNotRealized @event)
        {
            Status = BuildingUnitStatus.NotRealized;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasCorrectedToRetired @event)
        {
            Status = BuildingUnitStatus.Retired;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasCorrectedToPlanned @event)
        {
            Status = BuildingUnitStatus.Planned;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasCorrectedToRealized @event)
        {
            Status = BuildingUnitStatus.Realized;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasRealized @event)
        {
            Status = BuildingUnitStatus.Realized;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasNotRealized @event)
        {
            Status = BuildingUnitStatus.NotRealized;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasRetired @event)
        {
            Status = BuildingUnitStatus.Retired;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasPlanned @event)
        {
            Status = BuildingUnitStatus.Planned;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitStatusWasRemoved @event)
        {
            Status = null;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasRetiredByParent @event)
        {
            Status = BuildingUnitStatus.Retired;
            IsRetiredByParent = true;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasNotRealizedByParent @event)
        {
            Status = BuildingUnitStatus.NotRealized;
            IsRetiredByParent = true;
            IsRetiredByBuilding = false;
        }

        private void When(BuildingUnitWasNotRealizedByBuilding @event)
        {
            Status = BuildingUnitStatus.NotRealized;
            IsRetiredByBuilding = true;
        }

        #endregion Status

        private void When(BuildingUnitWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(BuildingUnitWasAdded @event)
        {
            _buildingId = new BuildingId(@event.BuildingId);
            BuildingUnitId = new BuildingUnitId(@event.BuildingUnitId);
            BuildingUnitKey = new BuildingUnitKey(@event.BuildingUnitKey);
            Function = BuildingUnitFunction.Unknown;
            AddressIds = new List<AddressId> { new AddressId(@event.AddressId) };
            BuildingGeometry = _parent.Geometry;

            Version = @event.BuildingUnitVersion;
        }

        private void When(BuildingUnitWasAddedToRetiredBuilding @event)
        {
            _buildingId = new BuildingId(@event.BuildingId);
            BuildingUnitId = new BuildingUnitId(@event.BuildingUnitId);
            BuildingUnitKey = new BuildingUnitKey(@event.BuildingUnitKey);
            Function = BuildingUnitFunction.Unknown;
            PreviousAddressId = new AddressId(@event.AddressId);
            BuildingGeometry = _parent.Geometry;

            Version = @event.BuildingUnitVersion;
        }

        private void When(CommonBuildingUnitWasAdded @event)
        {
            _buildingId = new BuildingId(@event.BuildingId);
            BuildingUnitId = new BuildingUnitId(@event.BuildingUnitId);
            BuildingUnitKey = new BuildingUnitKey(@event.BuildingUnitKey);
            Function = BuildingUnitFunction.Common;
            BuildingGeometry = _parent.Geometry;

            Version = @event.BuildingUnitVersion;
        }

        private void When(BuildingUnitWasReaddedByOtherUnitRemoval @event)
        {
            _buildingId = new BuildingId(@event.BuildingId);
            BuildingUnitId = new BuildingUnitId(@event.BuildingUnitId);
            BuildingUnitKey = new BuildingUnitKey(@event.BuildingUnitKey);
            Function = BuildingUnitFunction.Unknown;
            AddressIds = new List<AddressId> { new AddressId(@event.AddressId) };
            BuildingGeometry = _parent.Geometry;

            Version = @event.BuildingUnitVersion;
        }

        internal void CopyStateFrom(BuildingUnit predecessor)
        {
            _houseNumberPositionsFromCrab = predecessor._houseNumberPositionsFromCrab;
            _subaddressPositionsFromCrab = predecessor._subaddressPositionsFromCrab;

            _houseNumberStatusChronicle = predecessor._houseNumberStatusChronicle;
            _subaddressStatusChronicle = predecessor._subaddressStatusChronicle;

            _readdressedEvents = predecessor._readdressedEvents;

            CheckAndCorrectPositionIfNeeded(false, false);

            if (_subaddressStatusChronicle.Any())
            {
                ApplyStatusChangesFor(_subaddressStatusChronicle, null);
            }
            else
            {
                ApplyStatusChangesFor(_houseNumberStatusChronicle, null);
            }
        }

        #region CRAB
        private void When(AddressSubadressStatusWasImportedFromCrab @event)
        {
            _subaddressStatusChronicle.Add(@event);
        }

        private void When(AddressHouseNumberStatusWasImportedFromCrab @event)
        {
            _houseNumberStatusChronicle.Add(@event);
        }

        private void When(AddressSubaddressPositionWasImportedFromCrab @event)
        {
            if (_houseNumberPositionsFromCrab.Any())
                throw new InvalidOperationException("Cannot add crab house number event with already subaddress events present");

            _subaddressPositionsFromCrab.Add(@event);
        }

        private void When(AddressHouseNumberPositionWasImportedFromCrab @event)
        {
            if (_subaddressPositionsFromCrab.Any())
                throw new InvalidOperationException("Cannot add crab subaddress event with already house number events present");

            _houseNumberPositionsFromCrab.Add(@event);
        }

        private void When(AddressSubaddressWasImportedFromCrab @event)
        {
            if (@event.EndDateTime.HasValue && HasRetiredState)
                IsRetiredBySelf = true;
            else if (!@event.EndDateTime.HasValue)
                IsRetiredBySelf = false;
        }
        #endregion
    }
}
