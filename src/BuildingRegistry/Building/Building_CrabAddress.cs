namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events;
    using Events.Crab;
    using NodaTime;
    using System;
    using System.Linq;
    using ValueObjects;
    using ValueObjects.Crab;

    public partial class Building : AggregateRootEntity
    {
        public static readonly Func<Building> Factory = () => new Building();

        public static Building Register(BuildingId id)
        {
            var building = Factory();
            building.ApplyChange(new BuildingWasRegistered(id));
            return building;
        }

        public void ImportTerrainObjectHouseNumberFromCrab(
             CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
             CrabTerrainObjectId terrainObjectId,
             CrabHouseNumberId houseNumberId,
             CrabLifetime lifetime,
             CrabTimestamp timestamp,
             CrabOperator @operator,
             CrabModification? modification,
             CrabOrganisation? organisation)
        {
            GuardDeletedBuildingForCrab(modification);

            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId);

            if (modification == CrabModification.Delete)
            {
                RemoveBuildingUnit(buildingUnitKey, timestamp);
                ApplyCommonBuildingUnitRemoveIfNeeded();
            }
            else
            {
                var doNothing = false;
                var addressId = AddressId.CreateFor(houseNumberId);
                var predecessor = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);

                ApplyAddressChange(buildingUnitKey, houseNumberId, timestamp, modification);

                if (lifetime.EndDateTime.HasValue && predecessor != null && IsRetired && predecessor.IsRetiredByBuilding)
                {
                    foreach (var unit in _buildingUnitCollection.GetLastRetiredSubaddressBuildingUnitsByKeyParts(buildingUnitKey).Where(x => x.IsRetiredByBuilding))
                        unit.ApplyRetiredFromParent(predecessor.BuildingUnitId);

                    predecessor.ApplyRetired(modification == CrabModification.Correction);
                }
                else if (lifetime.EndDateTime.HasValue && ((predecessor != null && predecessor.HasRetiredState) || IsHouseNumberReaddressedAt(buildingUnitKey, timestamp)) && !_buildingUnitCollection.IsAddressLinkedToCommonBuildingUnit(addressId))
                {
                    doNothing = true;
                }
                else
                {
                    ImportHouseNumberBuildingUnit(terrainObjectId, houseNumberId, buildingUnitKey, addressId, modification, timestamp, lifetime.EndDateTime.HasValue);
                    _activeHouseNumberIdsByTerreinObjectHouseNr[terrainObjectHouseNumberId] = houseNumberId;
                }

                if (!doNothing && lifetime.EndDateTime.HasValue && !IsRetired && !IsHouseNumberReaddressedAt(buildingUnitKey, timestamp))
                {
                    predecessor = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                    foreach (var unit in _buildingUnitCollection.GetActiveSubaddressBuildingUnitsByKeyParts(buildingUnitKey))
                    {
                        if (!IsSubaddressReaddressedAt(BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId, new CrabSubaddressId(unit.BuildingUnitKey.Subaddress.Value)), timestamp))
                            unit.ApplyRetiredFromParent(predecessor.BuildingUnitId);
                    }

                    var buildingUnit = _buildingUnitCollection?.GetActiveUnitOrDefaultByKey(buildingUnitKey);

                    buildingUnit?.ApplyRetired(modification == CrabModification.Correction);

                    ApplyCommonBuildingUnitRetireIfNeeded(addressId);
                    if (_buildingUnitCollection.IsAddressLinkedToCommonBuildingUnit(addressId))
                    {
                        ApplyChange(new BuildingUnitAddressWasDetached(_buildingId, addressId,
                            _buildingUnitCollection.ActiveCommonBuildingUnit.BuildingUnitId));
                    }
                }

                if (_legacySubaddressEventsByTerreinObjectHouseNumber.ContainsKey(Tuple.Create(terrainObjectHouseNumberId, houseNumberId)))
                {
                    var addedBuildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                    foreach (var importedSubaddresses in _legacySubaddressEventsByTerreinObjectHouseNumber[Tuple.Create(terrainObjectHouseNumberId, houseNumberId)].GroupBy(x => x.SubaddressId))
                    {
                        var importedSubaddress = importedSubaddresses
                            .OrderBy(x => x.Timestamp)
                            .Last();

                        var addressIdForSubaddress = AddressId.CreateFor(new CrabSubaddressId(importedSubaddress.SubaddressId));
                        var subaddressLaterImported = _legacySubaddressEventsByTerreinObjectHouseNumber.Values
                            .SelectMany(x => x)
                            .Any(x =>
                                x.Timestamp > importedSubaddress.Timestamp &&
                                x.SubaddressId == importedSubaddress.SubaddressId);

                        var buildingUnitKeySubaddress = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId, new CrabSubaddressId(importedSubaddress.SubaddressId));
                        if (subaddressLaterImported && _buildingUnitCollection.ActiveBuildingUnits.Any(x => x.AddressIds.Contains(addressIdForSubaddress)))
                            continue;

                        ImportSubaddressFromCrab(
                            new CrabTerrainObjectId(importedSubaddress.TerrainObjectId),
                            new CrabTerrainObjectHouseNumberId(importedSubaddress.TerrainObjectHouseNumberId),
                            new CrabHouseNumberId(importedSubaddress.HouseNumberId),
                            new CrabSubaddressId(importedSubaddress.SubaddressId),
                            new CrabLifetime(importedSubaddress.BeginDateTime, importedSubaddress.EndDateTime),
                            timestamp,
                            importedSubaddress.Modification);
                    }

                    addedBuildingUnit.ApplyStatusChange((AddressHouseNumberStatusWasImportedFromCrab)null);
                }

                if (_buildingUnitCollection.HasReaddressed(buildingUnitKey) && _buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey))
                {
                    var children = _buildingUnitCollection.GetRetiredBuildingUnitsByKeyAndChildKeys(buildingUnitKey).Where(c => c.IsRetiredByParent && !c.IsRetiredBySelf);

                    foreach (var child in children)
                    {
                        if(_buildingUnitCollection.HasActiveUnitByKey(child.BuildingUnitKey))
                            continue;

                        ApplyAddBuildingUnit(_buildingUnitCollection.GetNextBuildingUnitIdFor(child.BuildingUnitKey), child.BuildingUnitKey, child.PreviousAddressId, new BuildingUnitVersion(timestamp));
                        ApplyCreateCommonBuildingUnitIfNeeded(terrainObjectId, new BuildingUnitVersion(timestamp));
                        ApplyAddressMoveToCommonIfNeeded(buildingUnitKey, modification);
                    }
                }
            }

            ApplyChange(new TerrainObjectHouseNumberWasImportedFromCrab(
                terrainObjectHouseNumberId,
                terrainObjectId,
                houseNumberId,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        private void ImportHouseNumberBuffer(BuildingUnitKey buildingUnitKey, AddressId addressId)
        {
            var addedBuildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
            if (_legacyHouseNumberPositionEventsByHouseNumberId.ContainsKey(addressId))
                addedBuildingUnit.ApplyPositionChange((AddressHouseNumberPositionWasImportedFromCrab)null, false);

            if (_legacyHouseNumberStatusEventsByHouseNumberId.ContainsKey(addressId))
                addedBuildingUnit.ApplyStatusChange((AddressHouseNumberStatusWasImportedFromCrab)null);
        }

        public void ImportHouseNumberStatusFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId);

            var legacyEvent = new AddressHouseNumberStatusWasImportedFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                houseNumberStatusId,
                houseNumberId,
                addressStatus,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            if (!IsHouseNumberReaddressedAt(buildingUnitKey, timestamp))
            {
                if (!_buildingUnitCollection.IsDeleted(buildingUnitKey) && _buildingUnitCollection.HasKey(buildingUnitKey))
                {
                    var buildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                    buildingUnit.ApplyStatusChange(legacyEvent);
                }
            }

            ApplyChange(legacyEvent);
        }

        public void ImportHouseNumberPositionFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabAddressPositionId addressPositionId,
            CrabHouseNumberId houseNumberId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId);
            var legacyEvent = new AddressHouseNumberPositionWasImportedFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                addressPositionId,
                houseNumberId,
                addressPosition,
                addressPositionOrigin,
                addressNature,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            if (!IsHouseNumberReaddressedAt(buildingUnitKey, timestamp) &&
                !_buildingUnitCollection.IsDeleted(buildingUnitKey) &&
                _buildingUnitCollection.HasKey(buildingUnitKey))
            {
                var buildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                buildingUnit.ApplyPositionChange(legacyEvent, legacyEvent.Modification == CrabModification.Correction);
            }

            ApplyChange(legacyEvent);
        }

        public void ImportSubaddressFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            if (IsRemoved)
                return;

            var legacyEvent = new AddressSubaddressWasImportedFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                subaddressId,
                houseNumberId,
                boxNumber,
                boxNumberType,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ImportSubaddressFromCrab(terrainObjectId, terrainObjectHouseNumberId, houseNumberId, subaddressId, lifetime, timestamp, modification);

            ApplyChange(legacyEvent);
        }

        private void ImportSubaddressFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabHouseNumberId houseNumberId,
            CrabSubaddressId subaddressId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabModification? modification)
        {
            var houseNumberKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId);
            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId, subaddressId);

            if (modification == CrabModification.Delete ||
                (_buildingUnitCollection.HasActiveUnitByKey(houseNumberKey) && (
                     _activeHouseNumberIdsByTerreinObjectHouseNr.ContainsKey(terrainObjectHouseNumberId) &&
                     _activeHouseNumberIdsByTerreinObjectHouseNr[terrainObjectHouseNumberId] != houseNumberId)))
            {
                RemoveBuildingUnit(buildingUnitKey, timestamp);
            }
            else if (_buildingUnitCollection.HasActiveUnitByKey(houseNumberKey) || _buildingUnitCollection.HasRetiredUnitByKey(houseNumberKey))
            {
                var addressId = AddressId.CreateFor(subaddressId);
                if (
                    (!lifetime.EndDateTime.HasValue &&
                     ((!_buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey) && _buildingUnitCollection.HasActiveUnitByKey(houseNumberKey)) ||
                      (!_buildingUnitCollection.HasActiveUnitByKey(houseNumberKey) && !_buildingUnitCollection.HasRetiredUnitByKey(buildingUnitKey))
                    )
                    || (lifetime.EndDateTime.HasValue && !_buildingUnitCollection.HasKey(buildingUnitKey))) //Even if retired, but never imported => import
                    && _activeHouseNumberIdsByTerreinObjectHouseNr.ContainsKey(terrainObjectHouseNumberId)) //but not if housenr isn't active
                {
                    ImportSubaddressBuildingUnit(terrainObjectId, buildingUnitKey, addressId, modification, timestamp,
                        lifetime.EndDateTime.HasValue);
                }

                if (lifetime.EndDateTime.HasValue && _buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey) && !IsSubaddressReaddressedAt(buildingUnitKey, timestamp))
                {
                    var buildingUnit = _buildingUnitCollection.GetActiveUnitByKey(buildingUnitKey);

                    buildingUnit.ApplyRetired(modification == CrabModification.Correction);

                    ApplyAddressMoveFromCommonIfNeeded(buildingUnitKey, new BuildingUnitVersion(timestamp), false);

                    ApplyCommonBuildingUnitRetireIfNeeded(addressId);
                }
            }
        }

        private void ImportSubaddressBuffer(BuildingUnitKey buildingUnitKey)
        {
            var subaddressId = new CrabSubaddressId(buildingUnitKey.Subaddress.Value);
            var buildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
            if (_legacySubaddressPositionEventsBySubadresId.ContainsKey(subaddressId))
                buildingUnit.ApplyPositionChange((AddressSubaddressPositionWasImportedFromCrab)null, false);

            if (_legacySubaddressStatusEventsBySubadresId.ContainsKey(subaddressId))
                buildingUnit.ApplyStatusChange((AddressSubaddressStatusWasImportedFromCrab)null);
        }

        public void ImportSubaddressStatusFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressStatusId subaddressStatusId,
            CrabSubaddressId subaddressId,
            CrabAddressStatus subaddressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId, subaddressId);
            var legacyEvent = new AddressSubaddressStatusWasImportedFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                subaddressStatusId,
                subaddressId,
                subaddressStatus,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            if (!IsSubaddressReaddressedAt(buildingUnitKey, timestamp) &&
                !_buildingUnitCollection.IsDeleted(buildingUnitKey) &&
                _buildingUnitCollection.HasKey(buildingUnitKey))
            {
                var buildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                buildingUnit.ApplyStatusChange(legacyEvent);
            }

            ApplyChange(legacyEvent);
        }

        public void ImportSubaddressPositionFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabAddressPositionId addressPositionId,
            CrabSubaddressId subaddressId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId, terrainObjectHouseNumberId, subaddressId);
            var legacyEvent = new AddressSubaddressPositionWasImportedFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                addressPositionId,
                subaddressId,
                addressPosition,
                addressPositionOrigin,
                addressNature,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            if (!IsSubaddressReaddressedAt(buildingUnitKey, timestamp) &&
                !_buildingUnitCollection.IsDeleted(buildingUnitKey) &&
                _buildingUnitCollection.HasKey(buildingUnitKey))
            {
                var buildingUnit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
                buildingUnit.ApplyPositionChange(legacyEvent, legacyEvent.Modification == CrabModification.Correction);
            }

            ApplyChange(legacyEvent);
        }

        public void ImportReaddressHouseNumberFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabAddressNature oldAddressNature,
            CrabHouseNumberId oldHouseNumberId,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId,
            CrabAddressNature newAddressNature,
            CrabHouseNumberId newHouseNumberId)
        {
            ApplyReaddress(terrainObjectId, oldTerrainObjectHouseNumberId, oldHouseNumberId, newHouseNumberId, beginDate);

            ApplyChange(
                new HouseNumberWasReaddressedFromCrab(
                    terrainObjectId,
                    readdressingId,
                    beginDate,
                    oldTerrainObjectHouseNumberId,
                    oldAddressNature,
                    oldHouseNumberId,
                    newTerrainObjectHouseNumberId,
                    newAddressNature,
                    newHouseNumberId));
        }

        public void ImportReaddressSubaddressFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabAddressNature oldAddressNature,
            CrabSubaddressId oldSubaddressId,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId,
            CrabAddressNature newAddressNature,
            CrabSubaddressId newSubaddressId)
        {
            ApplyReaddress(terrainObjectId, oldTerrainObjectHouseNumberId, oldSubaddressId, newSubaddressId, beginDate);

            ApplyChange(
                new SubaddressWasReaddressedFromCrab(
                    terrainObjectId,
                    readdressingId,
                    beginDate,
                    oldTerrainObjectHouseNumberId,
                    oldAddressNature,
                    oldSubaddressId,
                    newTerrainObjectHouseNumberId,
                    newAddressNature,
                    newSubaddressId));
        }

        private void ApplyReaddress(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabHouseNumberId oldHouseNumberId,
            CrabHouseNumberId newHouseNumberId,
            ReaddressingBeginDate beginDate)
        {
            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(
                BuildingUnitKey.Create(terrainObjectId, oldTerrainObjectHouseNumberId));

            var addressId = AddressId.CreateFor(oldHouseNumberId);

            if (unit != null)
            {
                ApplyChange(
                    new BuildingUnitWasReaddressed(
                        _buildingId,
                        unit.BuildingUnitId,
                        addressId,
                        AddressId.CreateFor(newHouseNumberId),
                        beginDate));
            }

            if (_buildingUnitCollection.ActiveCommonBuildingUnit != null &&
                _buildingUnitCollection.ActiveCommonBuildingUnit.AddressIds.Any(x => x == addressId))
            {
                ApplyChange(
                    new BuildingUnitWasReaddressed(
                        _buildingId,
                        _buildingUnitCollection.ActiveCommonBuildingUnit.BuildingUnitId,
                        addressId,
                        AddressId.CreateFor(newHouseNumberId),
                        beginDate));
            }
        }

        private void ApplyReaddress(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabSubaddressId oldSubaddressId,
            CrabSubaddressId newSubaddressId,
            ReaddressingBeginDate beginDate)
        {
            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(
                BuildingUnitKey.Create(terrainObjectId, oldTerrainObjectHouseNumberId, oldSubaddressId));

            var addressId = AddressId.CreateFor(oldSubaddressId);

            if (unit != null)
            {
                ApplyChange(
                    new BuildingUnitWasReaddressed(
                        _buildingId,
                        unit.BuildingUnitId,
                        addressId,
                        AddressId.CreateFor(newSubaddressId),
                        beginDate));
            }
        }

        private void RemoveBuildingUnit(BuildingUnitKey buildingUnitKey, CrabTimestamp timestamp)
        {
            foreach (var activeBuildingUnit in _buildingUnitCollection.GetActiveBuildingUnitsByKeyAndChildKeys(buildingUnitKey).OrderByDescending(x => x.BuildingUnitKey.Subaddress.HasValue))
            {
                var keyToMoveAddressFrom = activeBuildingUnit.BuildingUnitKey;
                if (_buildingUnitCollection.HasBeenReaddressed(activeBuildingUnit.BuildingUnitKey))
                    keyToMoveAddressFrom = _buildingUnitCollection.GetOldReaddressedKeyByUnitKey(activeBuildingUnit.BuildingUnitKey);

                activeBuildingUnit.ApplyRemove();
                ApplyAddressMoveFromCommonIfNeeded(keyToMoveAddressFrom, new BuildingUnitVersion(timestamp), !buildingUnitKey.Subaddress.HasValue);
                ApplyCommonBuildingUnitRemoveIfNeeded();
            }

            foreach (var retiredBuildingUnit in _buildingUnitCollection.GetRetiredBuildingUnitsByKeyAndChildKeys(buildingUnitKey))
            {
                retiredBuildingUnit.ApplyRemove();
                ApplyCommonBuildingUnitRemoveIfNeeded();
            }
        }

        private void ImportHouseNumberBuildingUnit(
            CrabTerrainObjectId terrainObjectId,
            CrabHouseNumberId houseNumberId,
            BuildingUnitKey buildingUnitKey,
            AddressId addressId,
            CrabModification? modification,
            CrabTimestamp timestamp,
            bool isRetired,
            bool fromAddressChange = false)
        {
            var currentAddressId = addressId;
            if (_buildingUnitCollection.HasBeenReaddressed(buildingUnitKey))
                currentAddressId = AddressId.CreateFor(new CrabHouseNumberId(_readdressedHouseNumbers[buildingUnitKey].NewHouseNumberId));

            if (!_buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey) &&
                !(_buildingUnitCollection.ActiveCommonBuildingUnit?.AddressIds.Contains(currentAddressId) ?? false))
            {
                var buildingUnitId = _buildingUnitCollection.GetNextBuildingUnitIdFor(buildingUnitKey, fromAddressChange);

                ApplyAddBuildingUnit(buildingUnitId, buildingUnitKey, addressId, new BuildingUnitVersion(timestamp));
                ImportHouseNumberBuffer(buildingUnitKey, AddressId.CreateFor(houseNumberId)); //TODO: Possible duplicate events

                if (isRetired)
                    return;

                ApplyCreateCommonBuildingUnitIfNeeded(terrainObjectId, new BuildingUnitVersion(timestamp));
                ApplyAddressMoveToCommonIfNeeded(buildingUnitKey, modification);

                //var children = _buildingUnitCollection.GetRetiredBuildingUnitsByKeyAndChildKeys(buildingUnitKey).Where(c => c.IsRetiredByParent && !c.IsRetiredBySelf);

                //foreach (var child in children)
                //{
                //    ApplyAddBuildingUnit(_buildingUnitCollection.GetNextBuildingUnitIdFor(child.BuildingUnitKey), child.BuildingUnitKey, child.PreviousAddressId, new BuildingUnitVersion(timestamp));
                //    ApplyCreateCommonBuildingUnitIfNeeded(terrainObjectId, new BuildingUnitVersion(timestamp));
                //    ApplyAddressMoveToCommonIfNeeded(buildingUnitKey, modification);
                //}
            }
        }

        private void ApplyAddressChange(
            BuildingUnitKey buildingUnitKey,
            CrabHouseNumberId houseNumberId,
            CrabTimestamp timestamp,
            CrabModification? modification)
        {
            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
            var oldAddressId = unit?.GetCurrentAddressIdsFor(timestamp) ?? unit?.PreviousAddressId;
            var newAddressId = AddressId.CreateFor(houseNumberId);

            if (unit != null && _buildingUnitCollection.HasReaddressed(buildingUnitKey))
            {
                var readdressedToKey = _buildingUnitCollection.GetNewReaddressedKeyByUnitKey(buildingUnitKey);
                var readdressedEvent = _readdressedHouseNumbers[readdressedToKey];
                if (readdressedToKey?.HouseNumber != unit.BuildingUnitKey.HouseNumber || (readdressedToKey.HouseNumber == unit.BuildingUnitKey.HouseNumber && houseNumberId == readdressedEvent.NewHouseNumberId))
                    return;
            }

            if (unit != null && oldAddressId != newAddressId && !IsHouseNumberReaddressedAt(buildingUnitKey, timestamp))
            {
                var terrainObjectId = new CrabTerrainObjectId(buildingUnitKey.Building);

                RemoveBuildingUnit(buildingUnitKey, timestamp);
                ImportHouseNumberBuildingUnit(terrainObjectId, houseNumberId, buildingUnitKey, newAddressId, modification, timestamp, false, true);
            }
        }

        private void ImportSubaddressBuildingUnit(
            CrabTerrainObjectId terrainObjectId,
            BuildingUnitKey buildingUnitKey,
            AddressId addressId,
            CrabModification? modification,
            CrabTimestamp timestamp,
            bool isRetired)
        {
            if (!_buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey))
            {
                var buildingUnitId = _buildingUnitCollection.GetNextBuildingUnitIdFor(buildingUnitKey, true);

                ApplyAddBuildingUnit(buildingUnitId, buildingUnitKey, addressId, new BuildingUnitVersion(timestamp));
                ImportSubaddressBuffer(buildingUnitKey);

                if (isRetired)
                    return;

                var parent = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey.CreateParentKey());
                if (!IsRetired && parent != null && parent.HasRetiredState && !parent.IsCommon && _buildingUnitCollection
                        .GetActiveSubaddressBuildingUnitsByKeyParts(buildingUnitKey)
                        .All(x => x.BuildingUnitKey == buildingUnitKey))
                {
                    _buildingUnitCollection.GetActiveUnitByKey(buildingUnitKey)
                        .ApplyRetired(modification == CrabModification.Correction);
                }

                ApplyCreateCommonBuildingUnitIfNeeded(terrainObjectId, new BuildingUnitVersion(timestamp));
                ApplyAddressMoveToCommonIfNeeded(buildingUnitKey, modification);
            }
        }

        private void ApplyAddressMoveFromCommonIfNeeded(
            BuildingUnitKey buildingUnitKey,
            BuildingUnitVersion version,
            bool isDelete)
        {
            if (_buildingUnitCollection.GetActiveSubaddressBuildingUnitsByKeyParts(buildingUnitKey).Count() == 1
                && (!buildingUnitKey.Subaddress.HasValue || !_buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey.CreateParentKey())))
            {
                var houseNumberKey = buildingUnitKey.ToHouseNumberKey();
                var retiredUnit = _buildingUnitCollection.GetRetiredUnitsByKey(houseNumberKey).OrderBy(x => x.Version).Last();
                var previousAddressId = retiredUnit.PreviousAddressId;

                _buildingUnitCollection.ActiveCommonBuildingUnit.ApplyDetachAddress(previousAddressId);

                var buildingUnitId = _buildingUnitCollection.GetNextBuildingUnitIdFor(houseNumberKey);

                if (!isDelete)
                {
                    ApplyChange(new BuildingUnitWasReaddedByOtherUnitRemoval(_buildingId, buildingUnitId, houseNumberKey, previousAddressId, version, retiredUnit.BuildingUnitId));
                    _buildingUnitCollection.GetById(buildingUnitId).CopyStateFrom(retiredUnit);
                }
            }
        }

        private void ApplyAddressMoveToCommonIfNeeded(BuildingUnitKey buildingUnitKey, CrabModification? modification)
        {
            if (_buildingUnitCollection.GetActiveSubaddressBuildingUnitsByKeyParts(buildingUnitKey).Count() == 2
                && _buildingUnitCollection.HasActiveUnitByKey(buildingUnitKey))
            {
                var houseNumber = _buildingUnitCollection.GetActiveHouseNumberBuildingUnitByKeyParts(buildingUnitKey);
                var common = _buildingUnitCollection.ActiveCommonBuildingUnit;

                houseNumber.ApplyRetired(modification == CrabModification.Correction);
                // Retire detaches address

                common.ApplyAttachAddress(houseNumber.PreviousAddressId);
            }
        }

        private void ApplyCommonBuildingUnitRetireIfNeeded(AddressId addressId)
        {
            var countUnits = _buildingUnitCollection.ActiveBuildingUnits.Count();
            countUnits += _buildingUnitCollection.ActiveCommonBuildingUnit?.AddressIds.Count ?? 0;
            if (_buildingUnitCollection.IsAddressLinkedToCommonBuildingUnit(addressId))
                countUnits--;

            if (countUnits <= 2)
                _buildingUnitCollection.ActiveCommonBuildingUnit?.ApplyRetired(false);
        }

        private void ApplyCommonBuildingUnitRemoveIfNeeded()
        {
            if (_buildingUnitCollection.ActiveBuildingUnits.Count() == 2)
                _buildingUnitCollection.ActiveCommonBuildingUnit.ApplyRemove();
        }

        private void ApplyCreateCommonBuildingUnitIfNeeded(CrabTerrainObjectId terrainObjectId, BuildingUnitVersion version)
        {
            if (_buildingUnitCollection.ActiveBuildingUnits.Count() == 2)
            {
                var unitKey = BuildingUnitKey.Create(terrainObjectId);
                var commonBuildingUnitId = _buildingUnitCollection.GetNextBuildingUnitIdFor(unitKey);

                ApplyChange(new CommonBuildingUnitWasAdded(_buildingId, commonBuildingUnitId, unitKey, version));
                ApplyChange(new BuildingUnitWasRealized(_buildingId, commonBuildingUnitId));

                if (Geometry != null)
                    ApplyChange(new BuildingUnitPositionWasDerivedFromObject(_buildingId, commonBuildingUnitId, Geometry.Center));

                _buildingUnitCollection.GetById(commonBuildingUnitId).CheckCompleteness();
            }
        }

        private void ApplyAddBuildingUnit(BuildingUnitId buildingUnitId, BuildingUnitKey buildingUnitKey, AddressId addressId, BuildingUnitVersion version)
        {
            var predecessor = GetPredecessorFor(buildingUnitKey);

            if (IsRetired)
                ApplyChange(new BuildingUnitWasAddedToRetiredBuilding(_buildingId, buildingUnitId, buildingUnitKey, addressId, version, predecessor?.BuildingUnitId));
            else
                ApplyChange(new BuildingUnitWasAdded(_buildingId, buildingUnitId, buildingUnitKey, addressId, version, predecessor?.BuildingUnitId));

            if (buildingUnitKey.Subaddress.HasValue)
            {
                ImportSubaddressBuffer(buildingUnitKey);
            }
            else if (buildingUnitKey.HouseNumber.HasValue)
            {
                ImportHouseNumberBuffer(buildingUnitKey, addressId);
            }

            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(buildingUnitKey);
            if (Geometry != null && unit.BuildingUnitPosition == null)
                ApplyChange(new BuildingUnitPositionWasDerivedFromObject(_buildingId, buildingUnitId, Geometry.Center));

            if (_buildingUnitCollection.HasBeenReaddressed(buildingUnitKey))
            {
                _readdressedHouseNumbers.TryGetValue(buildingUnitKey, out var readdressHouseNumber);
                _readdressedSubaddresses.TryGetValue(buildingUnitKey, out var readdressSubaddress);

                if (readdressHouseNumber != null)
                    ApplyReaddress(
                        new CrabTerrainObjectId(readdressHouseNumber.TerrainObjectId),
                        new CrabTerrainObjectHouseNumberId(readdressHouseNumber.OldTerrainObjectHouseNumberId),
                        new CrabHouseNumberId(readdressHouseNumber.OldHouseNumberId),
                        new CrabHouseNumberId(readdressHouseNumber.NewHouseNumberId),
                        new ReaddressingBeginDate(readdressHouseNumber.BeginDate));
                else if (readdressSubaddress != null)
                    ApplyReaddress(
                        new CrabTerrainObjectId(readdressSubaddress.TerrainObjectId),
                        new CrabTerrainObjectHouseNumberId(readdressSubaddress.OldTerrainObjectHouseNumberId),
                        new CrabSubaddressId(readdressSubaddress.OldSubaddressId),
                        new CrabSubaddressId(readdressSubaddress.NewSubaddressId),
                        new ReaddressingBeginDate(readdressSubaddress.BeginDate));
                else
                    throw new NotImplementedException();
            }
        }

        private bool IsHouseNumberReaddressedAt(BuildingUnitKey buildingUnitKey, CrabTimestamp timestamp)
        {
            return _readdressedHouseNumbers.ContainsKey(buildingUnitKey) &&
                   _readdressedHouseNumbers[buildingUnitKey].BeginDate.AtMidnight() <= LocalDateTime.FromDateTime(((Instant)timestamp).ToDateTimeUtc());
        }

        private bool IsSubaddressReaddressedAt(BuildingUnitKey buildingUnitKey, CrabTimestamp timestamp)
        {
            return _readdressedSubaddresses.ContainsKey(buildingUnitKey) &&
                   _readdressedSubaddresses[buildingUnitKey].BeginDate.AtMidnight() <= LocalDateTime.FromDateTime(((Instant)timestamp).ToDateTimeUtc());
        }
    }
}
