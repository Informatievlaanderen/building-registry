namespace BuildingRegistry.Building
{
    using System.Linq;
    using Events;
    using Exceptions;

    public partial class Building
    {
        public void PlanBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position,
            BuildingUnitFunction function,
            bool hasDeviation)
        {
            GuardRemovedBuilding();
            GuardValidStatusses(BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized);

            if (_buildingUnits.HasPersistentLocalId(new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)))
            {
                throw new BuildingUnitPersistentLocalIdAlreadyExistsException();
            }

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            ApplyChange(new BuildingUnitWasPlannedV2(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition,
                function,
                hasDeviation));

            EnsureCommonBuildingUnit(addCommonBuildingUnit);
        }

        public void RealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Realized);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Realize();
        }

        public void NotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .NotRealize();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RetireBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.Realized, BuildingStatus.UnderConstruction);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Retire();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RemoveBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            _buildingUnits
                .GetByPersistentLocalId(buildingUnitPersistentLocalId)
                .Remove();

            if (_buildingUnits.HasCommonBuildingUnit()
                && !_buildingUnits.NonCommonBuildingUnits().Any())
            {
                ApplyChange(new BuildingUnitWasRemovedV2(
                    BuildingPersistentLocalId,
                    _buildingUnits.CommonBuildingUnit().BuildingUnitPersistentLocalId));
            }

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void RegularizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.Realized, BuildingStatus.UnderConstruction);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Regularize();
        }

        public void DeregulateBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.Realized, BuildingStatus.UnderConstruction);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .Deregulate();
        }

        public void CorrectRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectRealization();
        }

        public void CorrectNotRealizeBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Realized, BuildingStatus.Planned, BuildingStatus.UnderConstruction);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectNotRealization(BuildingGeometry);

            EnsureCommonBuildingUnit(addCommonBuildingUnit);
        }

        public void CorrectRetiredBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Realized);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectRetirement(BuildingGeometry);

            EnsureCommonBuildingUnit(addCommonBuildingUnit);
        }

        public void CorrectPositionBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.Realized, BuildingStatus.UnderConstruction);

            var buildingUnit = _buildingUnits.GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId);

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            buildingUnit.CorrectPosition(positionGeometryMethod, finalPosition);
        }

        public void CorrectRegularizationBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectRegularization();
        }

        public void CorrectDeregulationBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();
            GuardBuildingValidStatuses(BuildingStatus.UnderConstruction, BuildingStatus.Realized, BuildingStatus.Planned);

            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .CorrectDeregulation();
        }

        public void AttachAddressToBuildingUnit(
            IAddresses addresses,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .AttachAddress(addressPersistentLocalId, addresses);
        }

        public void DetachAddressFromBuildingUnit(
            IAddresses addresses,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddress(addressPersistentLocalId, addresses);
        }
        public void DetachAddressFromBuildingUnitBecauseAddressWasRejected(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRejected(addressPersistentLocalId);
        }

        public void DetachAddressFromBuildingUnitBecauseAddressWasRetired(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRetired(addressPersistentLocalId);
        }

        public void DetachAddressFromBuildingUnitBecauseAddressWasRemoved(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _buildingUnits
                .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
                .DetachAddressBecauseAddressWasRemoved(addressPersistentLocalId);
        }

        public void ReplaceAddressAttachmentFromBuildingUnitBecauseStreetNameWasReaddressed(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId,
            AddressPersistentLocalId newAddressPersistentLocalId)
        {
            _buildingUnits
                .GetByPersistentLocalId(buildingUnitPersistentLocalId)
                .ReplaceAddressAttachmentFromBuildingUnitBecauseStreetNameWasReaddressed(previousAddressPersistentLocalId, newAddressPersistentLocalId);
        }

        public void MoveBuildingUnitInto(
            Building sourceBuilding,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            GuardBuildingValidStatuses(BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized);

            var buildingUnit = sourceBuilding._buildingUnits.GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId);
            

            /*Doelgebouw
               De regels van status gebouweenheid vs gebouw blijven behouden.
               Een gerealiseerd gebouweenheid kan niet bestaan in een gepland gebouw.
               Een gehistoreerd gebouweenheid kan bestaan in een een gerealiseerd gebouw, maar niet in een gepland of in aanbouw zijnde gebouw

            Brongebouw
               Een gebouweenheid kan verplaatst worden vanuit een gepland, in aanbouw, gerealiseerd of gehistoreerd gebouw.
               Opm. bij een gehistoreerd gebouw zal het gebouweenheid ook gehistoreerd zijn en dus ook geen adreskoppelingen meer hebben.
            */


        }

        public void MoveBuildingUnitOutOf(BuildingPersistentLocalId destinationBuildingPersistentLocalId, BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            //GuardRemovedBuilding();
            //GuardBuildingValidStatuses(BuildingStatus.Realized);

            //_buildingUnits
            //    .GetNotRemovedByPersistentLocalId(buildingUnitPersistentLocalId)
            //    .Realize();
        }

        private void GuardBuildingValidStatuses(params BuildingStatus[] validStatuses)
        {
            if (!validStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
        }

        private void CorrectBuildingPosition()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if (commonBuildingUnit.BuildingUnitPosition.Geometry != BuildingGeometry.Center)
            {
                ApplyChange(new BuildingUnitPositionWasCorrected(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingGeometry.Center));
            }
        }

        private void EnsureCommonBuildingUnit(IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            if (_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || !_buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            if (!_buildingUnits.HasCommonBuildingUnit(excludeRemoved: false))
            {
                AddCommonBuildingUnit(addCommonBuildingUnit);
                return;
            }

            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit(excludeRemoved: false);
            if (commonBuildingUnit.IsRemoved)
            {
                ApplyChange(new BuildingUnitRemovalWasCorrected(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction
                        ? BuildingUnitStatus.Planned
                        : BuildingUnitStatus.Realized,
                    BuildingUnitFunction.Common,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    BuildingGeometry.Center,
                    commonBuildingUnit.HasDeviation));
            }
            else
            {
                CorrectCommonBuildingUnitToPlannedOrRealized();
            }
        }

        private void AddCommonBuildingUnit(IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            var commonBuildingUnitPersistentLocalId =
                new BuildingUnitPersistentLocalId(addCommonBuildingUnit.GenerateNextPersistentLocalId());

            var commonBuildingUnitStatus = BuildingStatus == BuildingStatus.Realized
                ? BuildingUnitStatus.Realized
                : BuildingUnitStatus.Planned;

            ApplyChange(new CommonBuildingUnitWasAddedV2(
                BuildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                commonBuildingUnitStatus,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                BuildingGeometry.Center,
                hasDeviation: false));

            addCommonBuildingUnit.AddForBuilding(BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId);
        }

        private void CorrectCommonBuildingUnitToPlannedOrRealized()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if ((BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction)
                && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
                ApplyChange(new BuildingUnitWasRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.Retired)
            {
                CorrectBuildingPosition();

                ApplyChange(new BuildingUnitWasCorrectedFromRetiredToRealized(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }

        private void NotRealizeOrRetireCommonBuildingUnit()
        {
            if (!_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || _buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit();

            if (commonBuildingUnit.Status == BuildingUnitStatus.Planned)
            {
                foreach (var addressPersistentLocalId in commonBuildingUnit.AddressPersistentLocalIds.ToList())
                {
                    ApplyChange(new BuildingUnitAddressWasDetachedV2(
                        BuildingPersistentLocalId,
                        commonBuildingUnit.BuildingUnitPersistentLocalId,
                        addressPersistentLocalId));
                }

                ApplyChange(new BuildingUnitWasNotRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
            else if (commonBuildingUnit.Status == BuildingUnitStatus.Realized)
            {
                foreach (var addressPersistentLocalId in commonBuildingUnit.AddressPersistentLocalIds.ToList())
                {
                    ApplyChange(new BuildingUnitAddressWasDetachedV2(
                        BuildingPersistentLocalId,
                        commonBuildingUnit.BuildingUnitPersistentLocalId,
                        addressPersistentLocalId));
                }

                ApplyChange(new BuildingUnitWasRetiredV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }

        public void RealizeUnplannedBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            ApplyChange(new BuildingUnitWasPlannedV2(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                BuildingGeometry.Center,
                BuildingUnitFunction.Unknown,
                hasDeviation: false));

            ApplyChange(new BuildingUnitWasRealizedV2(BuildingPersistentLocalId, buildingUnitPersistentLocalId));
            ApplyChange(new BuildingUnitAddressWasAttachedV2(BuildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId));
        }
    }
}
