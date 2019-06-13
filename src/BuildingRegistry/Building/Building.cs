namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Events;
    using Events.Crab;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;
    using ValueObjects.Crab;

    public partial class Building
    {
        private static readonly WKBReader WkbReader = WKBReaderFactory.Create();

        public void ImportTerrainObjectFromCrab(
           CrabTerrainObjectId terrainObjectId,
           CrabIdentifierTerrainObject identifierTerrainObject,
           CrabTerrainObjectNatureCode terrainObjectNatureCode,
           CrabCoordinate xCoordinate,
           CrabCoordinate yCoordinate,
           CrabBuildingNature buildingNature,
           CrabLifetime crabLifetime,
           CrabTimestamp timestamp,
           CrabOperator @operator,
           CrabModification? modification,
           CrabOrganisation? organisation)
        {
            var wasRetired = IsRetired;
            if (IsRemoved)
                throw new BuildingRemovedException($"Cannot change removed building for building id {_buildingId}");

            if (!IsRemoved && modification.HasValue && modification.Value == CrabModification.Delete)
            {
                ApplyChange(new BuildingWasRemoved(_buildingId, _buildingUnitCollection.GetAllBuildingUnitIdsToRemove()));
            }
            else
            {
                if (crabLifetime.EndDateTime.HasValue && !IsRetired)
                {
                    //Retire building will also create retire terrainobjecthousenumber command
                    if (modification == CrabModification.Correction)
                    {
                        ApplyStatusCorrectionChange(_status == BuildingStatus.Realized
                            ? BuildingStatus.Retired
                            : BuildingStatus.NotRealized);
                    }
                    else
                    {
                        ApplyStatusChange(_status == BuildingStatus.Realized
                            ? BuildingStatus.Retired
                            : BuildingStatus.NotRealized);
                    }
                }

                if (!crabLifetime.EndDateTime.HasValue && IsRetired)
                {
                    var buildingStatusWasImportedFromCrab = _statusChronicle.MostCurrent(null);
                    var buildingStatus = buildingStatusWasImportedFromCrab == null ? null : MapStatus(buildingStatusWasImportedFromCrab.BuildingStatus, modification, false);
                    if (modification == CrabModification.Correction)
                    {
                        ApplyStatusCorrectionChange(buildingStatus);
                    }
                    else
                        ApplyStatusChange(buildingStatus);

                    if (!IsRetired && wasRetired)
                        UnretireBuildingUnits(modification, timestamp);
                }
            }

            ApplyChange(new TerrainObjectWasImportedFromCrab(
                terrainObjectId,
                identifierTerrainObject,
                terrainObjectNatureCode,
                xCoordinate,
                yCoordinate,
                buildingNature,
                crabLifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        private void UnretireBuildingUnits(CrabModification? modification, CrabTimestamp timestamp)
        {
            var buildingUnits = _buildingUnitCollection.GetLastRetiredUnitPerKey();
            foreach (var buildingUnit in buildingUnits.Where(unit => unit.IsRetiredByBuilding))
            {
                ApplyAddBuildingUnit(_buildingUnitCollection.GetNextBuildingUnitIdFor(buildingUnit.BuildingUnitKey), buildingUnit.BuildingUnitKey, buildingUnit.PreviousAddressId, new BuildingUnitVersion(timestamp));
                ApplyCreateCommonBuildingUnitIfNeeded(new CrabTerrainObjectId(buildingUnit.BuildingUnitKey.Building), new BuildingUnitVersion(timestamp));
                ApplyAddressMoveToCommonIfNeeded(buildingUnit.BuildingUnitKey, modification);
            }
        }

        public void ImportBuildingStatusFromCrab(
            CrabBuildingStatusId buildingStatusId,
            CrabTerrainObjectId terrainObjectId,
            CrabBuildingStatus buildingStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardDeletedBuildingForCrab(modification);

            var legacyEvent = new BuildingStatusWasImportedFromCrab(
                buildingStatusId,
                terrainObjectId,
                buildingStatus,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            var crabStatusEvent = _statusChronicle.MostCurrent(legacyEvent);

            var newStatus = crabStatusEvent == null
                ? null
                : MapStatus(crabStatusEvent.BuildingStatus, crabStatusEvent.Modification, IsRetired);

            if (_status != newStatus)
            {
                var previousStatus = _status;
                if (crabStatusEvent != null && crabStatusEvent.Modification == CrabModification.Correction)
                {
                    ApplyStatusCorrectionChange(newStatus);
                }
                else
                {
                    ApplyStatusChange(newStatus);
                }

                if (previousStatus == null && !IsRetired)
                    UnretireBuildingUnits(modification, timestamp);
            }

            ApplyCompletionIfNecessary();
            ApplyChange(legacyEvent);
        }

        public void ImportBuildingGeometryFromCrab(
            CrabBuildingGeometryId buildingGeometryId,
            CrabTerrainObjectId terrainObjectId,
            WkbGeometry wkbGeometry,
            CrabBuildingGeometryMethod buildingGeometryMethod,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardDeletedBuildingForCrab(modification);

            var legacyEvent = new BuildingGeometryWasImportedFromCrab(
                buildingGeometryId,
                terrainObjectId,
                wkbGeometry,
                buildingGeometryMethod,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ApplyGeometryChanges(legacyEvent);

            ApplyCompletionIfNecessary();

            ApplyChange(legacyEvent);
        }

        private static BuildingStatus? MapStatus(CrabBuildingStatus buildingStatus, CrabModification? modification, bool isRetired)
        {
            if (modification == CrabModification.Delete)
                return null;

            if (isRetired)
            {
                if (buildingStatus == CrabBuildingStatus.BuildingPermitGranted ||
                    buildingStatus == CrabBuildingStatus.PermitRequested ||
                    buildingStatus == CrabBuildingStatus.UnderConstruction)
                    return BuildingStatus.NotRealized;
                if (buildingStatus == CrabBuildingStatus.InUse || buildingStatus == CrabBuildingStatus.OutOfUse)
                    return BuildingStatus.Retired;
            }
            else
            {
                if (buildingStatus == CrabBuildingStatus.BuildingPermitGranted ||
                    buildingStatus == CrabBuildingStatus.PermitRequested)
                    return BuildingStatus.Planned;
                if (buildingStatus == CrabBuildingStatus.UnderConstruction)
                    return BuildingStatus.UnderConstruction;
                if (buildingStatus == CrabBuildingStatus.InUse || buildingStatus == CrabBuildingStatus.OutOfUse)
                    return BuildingStatus.Realized;
            }

            throw new NotImplementedException();
        }

        private static BuildingGeometryMethod MapToBuildingGeometryMethod(CrabBuildingGeometryMethod buildingGeometryMethod)
        {
            switch (buildingGeometryMethod)
            {
                case CrabBuildingGeometryMethod.Outlined:
                    return BuildingGeometryMethod.Outlined;
                case CrabBuildingGeometryMethod.Survey:
                case CrabBuildingGeometryMethod.Grb:
                    return BuildingGeometryMethod.MeasuredByGrb;
            }

            throw new InvalidOperationException($"Could not map CrabBuildingGeometryMethod {buildingGeometryMethod} to BuildingGeometryMethod");
        }

        private void ApplyStatusChange(BuildingStatus? buildingStatus)
        {
            switch (buildingStatus)
            {
                case BuildingStatus.NotRealized:
                    ApplyChange(new BuildingWasNotRealized(_buildingId,
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));
                    break;
                case BuildingStatus.Planned:
                    ApplyChange(new BuildingWasPlanned(_buildingId));
                    break;
                case BuildingStatus.Retired:
                    ApplyChange(new BuildingWasRetired(_buildingId,
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));

                    break;
                case BuildingStatus.Realized:
                    ApplyChange(new BuildingWasRealized(_buildingId));
                    break;
                case BuildingStatus.UnderConstruction:
                    ApplyChange(new BuildingBecameUnderConstruction(_buildingId));
                    break;
                case null:
                    ApplyChange(new BuildingStatusWasRemoved(_buildingId));
                    break;
            }
        }

        private void ApplyStatusCorrectionChange(BuildingStatus? buildingStatus)
        {
            switch (buildingStatus)
            {
                case BuildingStatus.NotRealized:
                    ApplyChange(new BuildingWasCorrectedToNotRealized(_buildingId,
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));
                    break;
                case BuildingStatus.Planned:
                    ApplyChange(new BuildingWasCorrectedToPlanned(_buildingId));
                    break;
                case BuildingStatus.Retired:
                    ApplyChange(new BuildingWasCorrectedToRetired(_buildingId,
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                        _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));
                    break;
                case BuildingStatus.Realized:
                    ApplyChange(new BuildingWasCorrectedToRealized(_buildingId));
                    break;
                case BuildingStatus.UnderConstruction:
                    ApplyChange(new BuildingWasCorrectedToUnderConstruction(_buildingId));
                    break;
                case null:
                    ApplyChange(new BuildingStatusWasCorrectedToRemoved(_buildingId));
                    break;
            }
        }

        private void ApplyGeometryChanges(BuildingGeometryWasImportedFromCrab legacyEvent)
        {
            var crabGeometry = _geometryChronicle.MostCurrent(legacyEvent);

            var newGeometryMethod = crabGeometry == null
                ? (BuildingGeometryMethod?)null
                : MapToBuildingGeometryMethod(crabGeometry.BuildingGeometryMethod);

            var validGeometry = string.IsNullOrEmpty(crabGeometry?.Geometry) ? null : ExtendedWkbGeometry.CreateEWkb(crabGeometry.Geometry.ToByteArray());

            var newGeometry = IsGeometryValid(validGeometry) && newGeometryMethod.HasValue ? new BuildingGeometry(validGeometry, newGeometryMethod.Value) : null;

            if (newGeometry != Geometry)
            {
                if (Geometry != null && (legacyEvent.Modification == CrabModification.Delete || newGeometry == null))
                {
                    ApplyChange(new BuildingGeometryWasRemoved(_buildingId));
                    foreach (var unit in _buildingUnitCollection.ActiveBuildingUnits)
                    {
                        unit.CheckCompleteness();
                    }
                }

                if (newGeometry != null && legacyEvent.Modification != CrabModification.Delete)
                {
                    if (legacyEvent.Modification == CrabModification.Correction)
                    {
                        ApplyGeometryChangeCorrection(newGeometryMethod, validGeometry);
                    }
                    else
                    {
                        ApplyGeometryChange(newGeometryMethod, validGeometry);
                    }
                }

                foreach (var activeBuildingUnit in _buildingUnitCollection.ActiveBuildingUnits)
                {
                    activeBuildingUnit.CheckAndCorrectPositionIfNeeded(legacyEvent.Modification == CrabModification.Correction);
                }

                foreach (var retiredBuildingUnit in _buildingUnitCollection.GetLastRetiredUnitPerKey())
                {
                    if (!_buildingUnitCollection.HasActiveUnitByKey(retiredBuildingUnit.BuildingUnitKey))
                        retiredBuildingUnit.CheckAndCorrectPositionIfNeeded(legacyEvent.Modification == CrabModification.Correction);
                }
            }
        }

        private static bool IsGeometryValid(ExtendedWkbGeometry validGeometry)
        {
            if (validGeometry == null)
                return false;

            var geometry = WkbReader.Read(validGeometry);
            var validOp =
                new NetTopologySuite.Operation.Valid.IsValidOp(geometry)
                {
                    IsSelfTouchingRingFormingHoleValid = true
                };

            return validOp.IsValid;
        }

        private void ApplyGeometryChange(BuildingGeometryMethod? newGeometryMethod, ExtendedWkbGeometry validGeometry)
        {
            switch (newGeometryMethod)
            {
                case BuildingGeometryMethod.Outlined:
                    ApplyChange(new BuildingWasOutlined(_buildingId, validGeometry));
                    break;

                case BuildingGeometryMethod.MeasuredByGrb:
                    ApplyChange(new BuildingWasMeasuredByGrb(_buildingId, validGeometry));
                    break;
            }
        }

        private void ApplyGeometryChangeCorrection(BuildingGeometryMethod? newGeometryMethod, ExtendedWkbGeometry validGeometry)
        {
            switch (newGeometryMethod)
            {
                case BuildingGeometryMethod.Outlined:
                    ApplyChange(new BuildingOutlineWasCorrected(_buildingId, validGeometry));
                    break;

                case BuildingGeometryMethod.MeasuredByGrb:
                    ApplyChange(new BuildingMeasurementByGrbWasCorrected(_buildingId, validGeometry));
                    break;
            }
        }

        private void ApplyCompletionIfNecessary()
        {
            if ((_status == null || Geometry == null) && _isComplete)
                ApplyChange(new BuildingBecameIncomplete(_buildingId));
            else if (_status != null && Geometry != null && !_isComplete)
                ApplyChange(new BuildingBecameComplete(_buildingId));
        }

        private void GuardDeletedBuildingForCrab(CrabModification? modification)
        {
            if (IsRemoved && modification != CrabModification.Delete)
                throw new BuildingRemovedException($"Cannot change removed building for building id {_buildingId}");
        }

        public void AssignOsloIdForCrabTerrainObjectId(
            CrabTerrainObjectId terrainObjectId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate,
            List<AssignBuildingUnitOsloIdForCrabTerrainObjectId> buildingUnitOsloIds,
            IOsloIdGenerator osloIdGenerator)
        {
            if (_osloId != null)
                return;

            if (osloId == null)
            {
                ApplyChange(new BuildingOsloIdWasAssigned(_buildingId, new OsloId(osloIdGenerator.GenerateNextOsloId()), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));
            }
            else
            {
                var deduplicatedCollection = new DeduplicatedBuildingUnitOsloIdCollection(buildingUnitOsloIds);
                var usedIds = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>();

                ApplyChange(new BuildingOsloIdWasAssigned(_buildingId, osloId, assignmentDate));

                if (_buildingUnitCollection.ActiveCommonBuildingUnit != null)
                {
                    var activeCommonUnitOsloId = deduplicatedCollection
                        .Where(x => x.CrabTerrainObjectHouseNumberId == null && x.CrabSubaddressId == null)
                        .OrderByDescending(x => x.Index)
                        .First();

                    ApplyBuildingUnitOsloIdWithDuplicateCheck(_buildingUnitCollection.ActiveCommonBuildingUnit,
                        activeCommonUnitOsloId, deduplicatedCollection);

                    usedIds.Add(activeCommonUnitOsloId);
                }

                foreach (var activeUnit in _buildingUnitCollection.ActiveBuildingUnits.Where(x => !x.IsCommon))
                {
                    //check if subaddress is readdressed, query might differ
                    var osloQuery = deduplicatedCollection
                        .Where(x => x.CrabTerrainObjectHouseNumberId != null &&
                                    x.CrabTerrainObjectHouseNumberId == activeUnit.BuildingUnitKey.HouseNumber.Value);

                    if (_buildingUnitCollection.HasReaddressed(activeUnit.BuildingUnitKey))
                    {
                        osloQuery = osloQuery.Union(deduplicatedCollection.Where(
                            x => x.CrabTerrainObjectHouseNumberId != null &&
                                 x.CrabTerrainObjectHouseNumberId == _buildingUnitCollection.GetReaddressedKey(activeUnit.BuildingUnitKey)));
                    }

                    if (activeUnit.BuildingUnitKey.Subaddress.HasValue)
                        osloQuery = osloQuery.Where(x =>
                            x.CrabSubaddressId != null &&
                            x.CrabSubaddressId == activeUnit.BuildingUnitKey.Subaddress.Value);
                    else
                        osloQuery = osloQuery.Where(x => x.CrabSubaddressId == null);

                    var activeUnitOsloId = osloQuery
                        .OrderByDescending(x => x.Index)
                        .FirstOrDefault();

                    if (!activeUnit.BuildingUnitKey.Subaddress.HasValue && activeUnitOsloId == null)
                    {
                        //Possible only for housenumbers
                        //See https://vlaamseoverheid.atlassian.net/wiki/spaces/GR/pages/469074524/Gebouwenregister+-+Mapping+Olso+Id+s+oude+-+nieuwe+architectuur. case 4
                        continue;
                    }
                    else if (activeUnitOsloId == null)
                    {
                        throw new InvalidOperationException("Cannot find oslo id for active subaddress");
                    }

                    ApplyBuildingUnitOsloIdWithDuplicateCheck(
                        activeUnit,
                        activeUnitOsloId,
                        deduplicatedCollection);

                    usedIds.Add(activeUnitOsloId);
                }

                var nonActiveUnitsWithoutOsloIdByKey = _buildingUnitCollection.GetAllBuildingUnitsWithoutOsloId()
                    .Where(x => x.IsRemoved || x.HasRetiredState)
                    .GroupBy(x => x.BuildingUnitKey)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var assignBuildingUnitOsloIdByUnit in deduplicatedCollection
                    .Except(usedIds)
                    .GroupBy(x => new { x.CrabTerrainObjectHouseNumberId, x.CrabSubaddressId }))
                {
                    var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId,
                        assignBuildingUnitOsloIdByUnit.Key.CrabTerrainObjectHouseNumberId,
                        assignBuildingUnitOsloIdByUnit.Key.CrabSubaddressId);

                    // less can only occurr for common units
                    //if (buildingUnitKey.HouseNumber.HasValue &&
                    //    (!nonActiveUnitsWithoutOsloIdByKey.ContainsKey(buildingUnitKey) || nonActiveUnitsWithoutOsloIdByKey[buildingUnitKey].Count < assignBuildingUnitOsloIdByUnit.Count()))
                    //    throw new InvalidOperationException("Should not occurr, more retired in previous is impossible except bug related to duplicates");

                    // Only common units can have none occurances => skip
                    if (!buildingUnitKey.HouseNumber.HasValue &&
                        !nonActiveUnitsWithoutOsloIdByKey.ContainsKey(buildingUnitKey))
                    {
                        foreach (var assignBuildingUnitOsloId in assignBuildingUnitOsloIdByUnit)
                            ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                _buildingId,
                                assignBuildingUnitOsloId.OsloId,
                                assignBuildingUnitOsloId.OsloAssignmentDate,
                                "Due to duplication the common building unit with this OsloId should never have existed."));
                    }

                    foreach (var assignBuildingUnitOsloId in assignBuildingUnitOsloIdByUnit.OrderByDescending(x =>
                        x.Index))
                    {
                        //https://vlaamseoverheid.atlassian.net/wiki/spaces/GR/pages/469074524/CRAB2VBR-mapping+OSLO+ID+s+oude+vs.+nieuwe+architectuur Case 5
                        var readdressedSubaddress = _readdressedSubaddresses
                            .FirstOrDefault(x => buildingUnitKey.Subaddress.HasValue &&
                                x.Value.OldSubaddressId == buildingUnitKey.Subaddress.Value &&
                                x.Value.NewTerrainObjectHouseNumberId == buildingUnitKey.HouseNumber.Value);

                        //https://vlaamseoverheid.atlassian.net/wiki/spaces/GR/pages/469074524/CRAB2VBR-mapping+OSLO+ID+s+oude+vs.+nieuwe+architectuur Case 6
                        if (_importedTerrainObjectHouseNumberIds.All(x => x != buildingUnitKey.HouseNumber))
                        {
                            if (buildingUnitKey.Subaddress.HasValue)
                            {
                                var readdressedSubaddressesWithWrongTerrainObjectHouseNr = _readdressedSubaddresses.Values.Where(x => x.OldSubaddressId == buildingUnitKey.Subaddress.Value).ToList();
                                if (readdressedSubaddressesWithWrongTerrainObjectHouseNr.Count == 1)
                                {
                                    var readdressed = readdressedSubaddressesWithWrongTerrainObjectHouseNr.First();
                                    var newKey = BuildingUnitKey.Create(terrainObjectId,
                                        new CrabTerrainObjectHouseNumberId(readdressed.NewTerrainObjectHouseNumberId),
                                        new CrabSubaddressId(readdressed.NewSubaddressId));

                                    var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(newKey);
                                    if (unit != null)
                                    {
                                        ApplyChange(new BuildingUnitOsloIdWasDuplicated(
                                            _buildingId,
                                            unit.BuildingUnitId,
                                            assignBuildingUnitOsloId.OsloId,
                                            unit.OsloId,
                                            assignBuildingUnitOsloId.OsloAssignmentDate));
                                    }
                                    else
                                    {
                                        ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                            _buildingId,
                                            assignBuildingUnitOsloId.OsloId,
                                            assignBuildingUnitOsloId.OsloAssignmentDate,
                                            "The building unit with this OsloId should never have existed. Could not mark as duplicate as no candidate was found."));
                                    }
                                }
                                else
                                {
                                    // cant determine which goes where if the address is connected by different terrainobject housenumbers
                                    ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                        _buildingId,
                                        assignBuildingUnitOsloId.OsloId,
                                        assignBuildingUnitOsloId.OsloAssignmentDate,
                                        "The building unit with this OsloId should never have existed. Could not mark as duplicate because multiple candidates were found."));
                                }
                            }
                            else
                            {
                                //no way to search as the address id (housenumberid) is not in key
                                ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitOsloId.OsloId,
                                    assignBuildingUnitOsloId.OsloAssignmentDate,
                                    "The building unit with this OsloId should never have existed. Due to insufficient data it can not be marked as duplicate."));
                            }
                        }
                        //case 5
                        else if (buildingUnitKey.Subaddress.HasValue
                            && !nonActiveUnitsWithoutOsloIdByKey.ContainsKey(buildingUnitKey)
                            && readdressedSubaddress.Key != null)
                        {
                            var newKey = BuildingUnitKey.Create(terrainObjectId,
                                new CrabTerrainObjectHouseNumberId(readdressedSubaddress.Value.NewTerrainObjectHouseNumberId),
                                new CrabSubaddressId(readdressedSubaddress.Value.NewSubaddressId));

                            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(newKey);
                            if (unit != null)
                            {
                                var oldOsloId = unit.OsloId;
                                unit.ApplyOsloId(
                                    assignBuildingUnitOsloId.OsloId,
                                    assignBuildingUnitOsloId.OsloAssignmentDate);

                                ApplyChange(new BuildingUnitOsloIdWasDuplicated(
                                    _buildingId,
                                    unit.BuildingUnitId,
                                    oldOsloId,
                                    unit.OsloId,
                                    assignBuildingUnitOsloId.OsloAssignmentDate));
                            }
                            else
                            {
                                ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitOsloId.OsloId,
                                    assignBuildingUnitOsloId.OsloAssignmentDate,
                                    "The building unit with this OsloId should never have existed. Could not mark as duplicate as no candidate was found."));
                            }
                        }
                        else if (!nonActiveUnitsWithoutOsloIdByKey.ContainsKey(buildingUnitKey))
                        {
                            ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                _buildingId,
                                assignBuildingUnitOsloId.OsloId,
                                assignBuildingUnitOsloId.OsloAssignmentDate,
                                "The building unit with this OsloId should never have existed due to readdressing. Could not mark as duplicate as no candidate was found."));
                        }
                        else
                        {
                            var buildingUnit = nonActiveUnitsWithoutOsloIdByKey[buildingUnitKey]
                                .LastOrDefault(x => x.OsloId == null);

                            // less can occurr
                            if (buildingUnit == null)
                                ApplyChange(new BuildingUnitOsloIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitOsloId.OsloId,
                                    assignBuildingUnitOsloId.OsloAssignmentDate,
                                    "Due to duplication, the building unit with this OsloId should never have existed. Could not mark as duplicate as no candidate was found."));

                            ApplyBuildingUnitOsloIdWithDuplicateCheck(
                                buildingUnit,
                                assignBuildingUnitOsloId,
                                deduplicatedCollection);
                        }
                    }
                }
            }

            foreach (var buildingUnit in _buildingUnitCollection.GetAllBuildingUnitsWithoutOsloId())
            {
                var id = buildingUnit.BuildingUnitId;
                ApplyChange(new BuildingUnitOsloIdWasAssigned(_buildingId, id, new OsloId(osloIdGenerator.GenerateNextOsloId()), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));
            }
        }

        private void ApplyBuildingUnitOsloIdWithDuplicateCheck(
            BuildingUnit buildingUnit,
            AssignBuildingUnitOsloIdForCrabTerrainObjectId assignOsloId,
            DeduplicatedBuildingUnitOsloIdCollection deduplicatedCollection)
        {
            buildingUnit.ApplyOsloId(
                assignOsloId.OsloId,
                assignOsloId.OsloAssignmentDate);

            if (deduplicatedCollection.HasDuplicate(assignOsloId))
            {
                var dupe = deduplicatedCollection.GetDuplicate(assignOsloId);
                ApplyChange(new BuildingUnitOsloIdWasDuplicated(
                    _buildingId,
                    buildingUnit.BuildingUnitId,
                    dupe.OsloId,
                    assignOsloId.OsloId,
                    dupe.OsloAssignmentDate));
            }
        }

        public void AssignOsloIds(IOsloIdGenerator osloIdGenerator)
        {
            if(_osloId == null)
                ApplyChange(new BuildingOsloIdWasAssigned(_buildingId, osloIdGenerator.GenerateNextOsloId(), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));

            foreach (var buildingUnit in _buildingUnitCollection.GetAllBuildingUnitsWithoutOsloId())
            {
                var id = buildingUnit.BuildingUnitId;
                ApplyChange(new BuildingUnitOsloIdWasAssigned(_buildingId, id, new OsloId(osloIdGenerator.GenerateNextOsloId()), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));

            }
        }
    }
}
