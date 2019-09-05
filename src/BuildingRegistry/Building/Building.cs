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
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
                    ApplyChange(
                        new BuildingWasNotRealized(
                            _buildingId,
                            _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                            _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));
                    break;

                case BuildingStatus.Planned:
                    ApplyChange(new BuildingWasPlanned(_buildingId));
                    break;

                case BuildingStatus.Retired:
                    ApplyChange(
                        new BuildingWasRetired(
                            _buildingId,
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
                    ApplyChange(
                        new BuildingWasCorrectedToNotRealized(
                            _buildingId,
                            _buildingUnitCollection.GetActiveBuildingUnitIdsToRetire(),
                            _buildingUnitCollection.GetActiveBuildingUnitIdsToNotRealized()));
                    break;

                case BuildingStatus.Planned:
                    ApplyChange(new BuildingWasCorrectedToPlanned(_buildingId));
                    break;

                case BuildingStatus.Retired:
                    ApplyChange(
                        new BuildingWasCorrectedToRetired(
                            _buildingId,
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
                        unit.CheckCompleteness();
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
                    activeBuildingUnit.CheckAndCorrectPositionIfNeeded(legacyEvent.Modification == CrabModification.Correction);

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

        public void AssignPersistentLocalIdForCrabTerrainObjectId(
            CrabTerrainObjectId terrainObjectId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate persistentLocalIdAssignmentDate,
            List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId> buildingUnitPersistentLocalIds,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
        {
            if (_persistentLocalId != null)
                return;

            if (persistentLocalId == null)
            {
                ApplyChange(new BuildingPersistentLocalIdWasAssigned(_buildingId, new PersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId()), new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));
            }
            else
            {
                var deduplicatedCollection = new DeduplicatedBuildingUnitPersistentLocalIdCollection(buildingUnitPersistentLocalIds);
                var usedIds = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>();

                ApplyChange(new BuildingPersistentLocalIdWasAssigned(_buildingId, persistentLocalId, persistentLocalIdAssignmentDate));

                if (_buildingUnitCollection.ActiveCommonBuildingUnit != null)
                {
                    var activeCommonUnitPersistentLocalId = deduplicatedCollection
                        .Where(x => x.CrabTerrainObjectHouseNumberId == null && x.CrabSubaddressId == null)
                        .OrderByDescending(x => x.Index)
                        .First();

                    ApplyBuildingUnitPersistentLocalIdWithDuplicateCheck(_buildingUnitCollection.ActiveCommonBuildingUnit,
                        activeCommonUnitPersistentLocalId, deduplicatedCollection);

                    usedIds.Add(activeCommonUnitPersistentLocalId);
                }

                foreach (var activeUnit in _buildingUnitCollection.ActiveBuildingUnits.Where(x => !x.IsCommon))
                {
                    //check if subaddress is readdressed, query might differ
                    var persistentLocalIdQuery = deduplicatedCollection
                        .Where(x => x.CrabTerrainObjectHouseNumberId != null &&
                                    x.CrabTerrainObjectHouseNumberId == activeUnit.BuildingUnitKey.HouseNumber.Value);

                    if (_buildingUnitCollection.HasReaddressed(activeUnit.BuildingUnitKey))
                    {
                        persistentLocalIdQuery = persistentLocalIdQuery.Union(deduplicatedCollection.Where(
                            x => x.CrabTerrainObjectHouseNumberId != null &&
                                 x.CrabTerrainObjectHouseNumberId == _buildingUnitCollection.GetReaddressedKey(activeUnit.BuildingUnitKey)));
                    }

                    if (activeUnit.BuildingUnitKey.Subaddress.HasValue)
                        persistentLocalIdQuery = persistentLocalIdQuery.Where(x =>
                            x.CrabSubaddressId != null &&
                            x.CrabSubaddressId == activeUnit.BuildingUnitKey.Subaddress.Value);
                    else
                        persistentLocalIdQuery = persistentLocalIdQuery.Where(x => x.CrabSubaddressId == null);

                    var activeUnitPersistentLocalId = persistentLocalIdQuery
                        .OrderByDescending(x => x.Index)
                        .FirstOrDefault();

                    if (!activeUnit.BuildingUnitKey.Subaddress.HasValue && activeUnitPersistentLocalId == null)
                    {
                        //Possible only for housenumbers
                        //See https://vlaamseoverheid.atlassian.net/wiki/spaces/GR/pages/469074524/Gebouwenregister+-+Mapping+Olso+Id+s+oude+-+nieuwe+architectuur. case 4
                        continue;
                    }
                    else if (activeUnitPersistentLocalId == null)
                    {
                        throw new InvalidOperationException("Cannot find persistent local id for active subaddress");
                    }

                    ApplyBuildingUnitPersistentLocalIdWithDuplicateCheck(
                        activeUnit,
                        activeUnitPersistentLocalId,
                        deduplicatedCollection);

                    usedIds.Add(activeUnitPersistentLocalId);
                }

                var nonActiveUnitsWithoutPersistentLocalIdByKey = _buildingUnitCollection.GetAllBuildingUnitsWithoutPersistentLocalId()
                    .Where(x => x.IsRemoved || x.HasRetiredState)
                    .GroupBy(x => x.BuildingUnitKey)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var assignBuildingUnitPersistentLocalIdByUnit in deduplicatedCollection
                    .Except(usedIds)
                    .GroupBy(x => new { x.CrabTerrainObjectHouseNumberId, x.CrabSubaddressId }))
                {
                    var buildingUnitKey = BuildingUnitKey.Create(terrainObjectId,
                        assignBuildingUnitPersistentLocalIdByUnit.Key.CrabTerrainObjectHouseNumberId,
                        assignBuildingUnitPersistentLocalIdByUnit.Key.CrabSubaddressId);

                    // Only common units can have none occurances => skip
                    if (!buildingUnitKey.HouseNumber.HasValue &&
                        !nonActiveUnitsWithoutPersistentLocalIdByKey.ContainsKey(buildingUnitKey))
                    {
                        foreach (var assignBuildingUnitPersistentLocalId in assignBuildingUnitPersistentLocalIdByUnit)
                            ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                _buildingId,
                                assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                new Reason("Due to duplication the common building unit with this PersistentLocalId should never have existed.")));
                    }

                    foreach (var assignBuildingUnitPersistentLocalId in assignBuildingUnitPersistentLocalIdByUnit.OrderByDescending(x => x.Index))
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
                                        ApplyChange(new BuildingUnitPersistentLocalIdWasDuplicated(
                                            _buildingId,
                                            unit.BuildingUnitId,
                                            assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                            unit.PersistentLocalId,
                                            assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate));
                                    }
                                    else
                                    {
                                        ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                            _buildingId,
                                            assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                            assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                            new Reason("The building unit with this PersistentLocalId should never have existed. Could not mark as duplicate as no candidate was found.")));
                                    }
                                }
                                else
                                {
                                    // cant determine which goes where if the address is connected by different terrainobject housenumbers
                                    ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                        _buildingId,
                                        assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                        assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                        new Reason("The building unit with this PersistentLocalId should never have existed. Could not mark as duplicate because multiple candidates were found.")));
                                }
                            }
                            else
                            {
                                //no way to search as the address id (housenumberid) is not in key
                                ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                    new Reason("The building unit with this PersistentLocalId should never have existed. Due to insufficient data it can not be marked as duplicate.")));
                            }
                        }
                        //case 5
                        else if (buildingUnitKey.Subaddress.HasValue
                            && !nonActiveUnitsWithoutPersistentLocalIdByKey.ContainsKey(buildingUnitKey)
                            && readdressedSubaddress.Key != null)
                        {
                            var newKey = BuildingUnitKey.Create(terrainObjectId,
                                new CrabTerrainObjectHouseNumberId(readdressedSubaddress.Value.NewTerrainObjectHouseNumberId),
                                new CrabSubaddressId(readdressedSubaddress.Value.NewSubaddressId));

                            var unit = _buildingUnitCollection.GetActiveOrLastRetiredByKey(newKey);
                            if (unit != null)
                            {
                                var oldPersistentLocalId = unit.PersistentLocalId;
                                unit.ApplyPersistentLocalId(
                                    assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate);

                                ApplyChange(new BuildingUnitPersistentLocalIdWasDuplicated(
                                    _buildingId,
                                    unit.BuildingUnitId,
                                    oldPersistentLocalId,
                                    unit.PersistentLocalId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate));
                            }
                            else
                            {
                                ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                    new Reason("The building unit with this PersistentLocalId should never have existed. Could not mark as duplicate as no candidate was found.")));
                            }
                        }
                        else if (!nonActiveUnitsWithoutPersistentLocalIdByKey.ContainsKey(buildingUnitKey))
                        {
                            ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                _buildingId,
                                assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                new Reason("The building unit with this PersistentLocalId should never have existed due to readdressing. Could not mark as duplicate as no candidate was found.")));
                        }
                        else
                        {
                            var buildingUnit = nonActiveUnitsWithoutPersistentLocalIdByKey[buildingUnitKey]
                                .LastOrDefault(x => x.PersistentLocalId == null);

                            // less can occurr
                            if (buildingUnit == null)
                                ApplyChange(new BuildingUnitPersistentLocalIdWasRemoved(
                                    _buildingId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalId,
                                    assignBuildingUnitPersistentLocalId.PersistentLocalIdAssignmentDate,
                                    new Reason("Due to duplication, the building unit with this PersistentLocalId should never have existed. Could not mark as duplicate as no candidate was found.")));

                            ApplyBuildingUnitPersistentLocalIdWithDuplicateCheck(
                                buildingUnit,
                                assignBuildingUnitPersistentLocalId,
                                deduplicatedCollection);
                        }
                    }
                }
            }

            foreach (var buildingUnit in _buildingUnitCollection.GetAllBuildingUnitsWithoutPersistentLocalId())
            {
                var id = buildingUnit.BuildingUnitId;
                ApplyChange(new BuildingUnitPersistentLocalIdWasAssigned(_buildingId, id, new PersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId()), new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));
            }
        }

        private void ApplyBuildingUnitPersistentLocalIdWithDuplicateCheck(
            BuildingUnit buildingUnit,
            AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId assignPersistentLocalId,
            DeduplicatedBuildingUnitPersistentLocalIdCollection deduplicatedCollection)
        {
            buildingUnit.ApplyPersistentLocalId(
                assignPersistentLocalId.PersistentLocalId,
                assignPersistentLocalId.PersistentLocalIdAssignmentDate);

            if (deduplicatedCollection.HasDuplicate(assignPersistentLocalId))
            {
                var dupe = deduplicatedCollection.GetDuplicate(assignPersistentLocalId);
                ApplyChange(new BuildingUnitPersistentLocalIdWasDuplicated(
                    _buildingId,
                    buildingUnit.BuildingUnitId,
                    dupe.PersistentLocalId,
                    assignPersistentLocalId.PersistentLocalId,
                    dupe.PersistentLocalIdAssignmentDate));
            }
        }

        public void AssignPersistentLocalIds(IPersistentLocalIdGenerator persistentLocalIdGenerator)
        {
            if (_persistentLocalId == null)
                ApplyChange(
                    new BuildingPersistentLocalIdWasAssigned(
                        _buildingId,
                        persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));

            foreach (var buildingUnit in _buildingUnitCollection.GetAllBuildingUnitsWithoutPersistentLocalId())
                ApplyChange(
                    new BuildingUnitPersistentLocalIdWasAssigned(
                        _buildingId,
                        buildingUnit.BuildingUnitId,
                        new PersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId()),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now))));
        }
    }
}
