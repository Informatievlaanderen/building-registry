namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public class BuildingUnitCollection
    {
        private readonly List<BuildingUnit> _allBuildingUnits = new List<BuildingUnit>();
        private readonly Dictionary<BuildingUnitKey, IList<BuildingUnit>> _allBuildingUnitsByKey = new Dictionary<BuildingUnitKey, IList<BuildingUnit>>();
        private readonly Dictionary<BuildingUnitKey, BuildingUnitKey> _readdressedKeys = new Dictionary<BuildingUnitKey, BuildingUnitKey>();
        private readonly Func<BuildingUnit, bool> _isRetiredPredicate = x => x.HasRetiredState && !x.IsRemoved;
        private readonly Func<BuildingUnit, bool> _isActivePredicate = x => !x.IsRemoved && !x.HasRetiredState;

        public IEnumerable<BuildingUnit> ActiveBuildingUnits => _allBuildingUnits.Where(_isActivePredicate);
        public BuildingUnit ActiveCommonBuildingUnit =>
            ActiveBuildingUnits.SingleOrDefault(x => x.Function == BuildingUnitFunction.Common);

        public bool HasBeenReaddressed(BuildingUnitKey buildingUnitKey)
            => _readdressedKeys.ContainsValue(buildingUnitKey);

        public BuildingUnitKey GetNewReaddressedKeyByUnitKey(BuildingUnitKey buildingUnitKey)
            => _readdressedKeys[buildingUnitKey];

        public BuildingUnitKey GetOldReaddressedKeyByUnitKey(BuildingUnitKey buildingUnitKey)
            => _readdressedKeys.Single(x => x.Value == buildingUnitKey).Key;

        public bool HasReaddressed(BuildingUnitKey buildingUnitKey)
            => _readdressedKeys.ContainsKey(buildingUnitKey);

        public bool HasRetiredUnitByKey(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return HasRetiredUnitByKey(_readdressedKeys[buildingUnitKey]);

            return _allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _allBuildingUnitsByKey[buildingUnitKey].Any(_isRetiredPredicate);
        }

        public IEnumerable<BuildingUnit> GetRetiredUnitsByKey(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return GetRetiredUnitsByKey(_readdressedKeys[buildingUnitKey]);

            return _allBuildingUnitsByKey[buildingUnitKey].Where(_isRetiredPredicate);
        }

        public IEnumerable<BuildingUnit> GetLastRetiredUnitPerKey()
        {
            return _allBuildingUnits
                .Where(_isRetiredPredicate)
                .GroupBy(x => x.BuildingUnitKey)
                .Select(x => x.OrderBy(y => y.Version).Last());
        }

        public BuildingUnit GetById(BuildingUnitId buildingUnitId)
        {
            return _allBuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);
        }

        public bool HasActiveUnitByKey(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return HasActiveUnitByKey(_readdressedKeys[buildingUnitKey]);

            return _allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _allBuildingUnitsByKey[buildingUnitKey].Any(_isActivePredicate);
        }

        public bool HasKey(BuildingUnitKey buildingUnitKey)
        {
            return _readdressedKeys.ContainsKey(buildingUnitKey) || _allBuildingUnitsByKey.ContainsKey(buildingUnitKey);
        }

        public bool IsDeleted(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return IsDeleted(_readdressedKeys[buildingUnitKey]);

            return _allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _allBuildingUnitsByKey[buildingUnitKey].Any(x => x.IsRemoved);
        }

        public IEnumerable<BuildingUnit> GetActiveSubaddressBuildingUnitsByKeyParts(BuildingUnitKey key)
        {
            var houseNrKey = key.ToHouseNumberKey();
            var units = ActiveBuildingUnits
                .Where(x => x.BuildingUnitKey.Building == houseNrKey.Building &&
                            x.BuildingUnitKey.HouseNumber == houseNrKey.HouseNumber &&
                            x.BuildingUnitKey.Subaddress.HasValue);

            var readdressedKey = _readdressedKeys.FirstOrDefault(x => x.Key.ToHouseNumberKey() == houseNrKey);
            if (readdressedKey.Value != null && readdressedKey.Value != key)
                units = units.Union(GetActiveSubaddressBuildingUnitsByKeyParts(readdressedKey.Value));

            return units;
        }

        public IEnumerable<BuildingUnit> GetLastRetiredSubaddressBuildingUnitsByKeyParts(BuildingUnitKey key)
        {
            var houseNrKey = key.ToHouseNumberKey();
            var buildingUnits = _allBuildingUnitsByKey
                .Where(x => x.Key.Building == houseNrKey.Building &&
                            x.Key.HouseNumber == houseNrKey.HouseNumber &&
                            x.Key.Subaddress.HasValue)
                .Select(x => x.Value.OrderBy(y => y.Version).Last());

            if (_readdressedKeys.ContainsKey(houseNrKey))
                buildingUnits = buildingUnits.Union(GetLastRetiredSubaddressBuildingUnitsByKeyParts(_readdressedKeys[houseNrKey]));

            return buildingUnits;
        }

        public BuildingUnit GetActiveHouseNumberBuildingUnitByKeyParts(BuildingUnitKey key)
        {
            var houseNrKey = key.ToHouseNumberKey();

            var unit = ActiveBuildingUnits
                .SingleOrDefault(x => x.BuildingUnitKey.Building == houseNrKey.Building &&
                             x.BuildingUnitKey.HouseNumber == houseNrKey.HouseNumber &&
                             !x.BuildingUnitKey.Subaddress.HasValue);

            if (unit != null)
                return unit;
            else if (_readdressedKeys.ContainsKey(houseNrKey))
                return GetActiveHouseNumberBuildingUnitByKeyParts(_readdressedKeys[houseNrKey]);

            throw new InvalidOperationException("Failed to look for a housenr that isn't available");
        }

        public BuildingUnit GetActiveUnitByKey(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return GetActiveUnitByKey(_readdressedKeys[buildingUnitKey]);

            return _allBuildingUnitsByKey[buildingUnitKey].Single(_isActivePredicate);
        }

        public BuildingUnit GetActiveUnitOrDefaultByKey(BuildingUnitKey buildingUnitKey)
        {
            return !HasActiveUnitByKey(buildingUnitKey) ? null : GetActiveUnitByKey(buildingUnitKey);
        }

        public BuildingUnit GetActiveOrLastRetiredByKey(BuildingUnitKey buildingUnitKey)
        {
            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _readdressedKeys.ContainsKey(buildingUnitKey))
                return GetActiveOrLastRetiredByKey(_readdressedKeys[buildingUnitKey]);

            if (ActiveCommonBuildingUnit?.BuildingUnitKey == buildingUnitKey)
                return ActiveCommonBuildingUnit;

            if (HasActiveUnitByKey(buildingUnitKey))
                return GetActiveUnitByKey(buildingUnitKey);

            if (HasRetiredUnitByKey(buildingUnitKey))
                return GetRetiredUnitsByKey(buildingUnitKey).OrderBy(x => x.Version).Last();

            return null;
        }

        public IList<BuildingUnit> GetRetiredBuildingUnitsByKeyAndChildKeys(BuildingUnitKey buildingUnitKey)
        {
            IEnumerable<BuildingUnit> buildingUnits = new List<BuildingUnit>();
            buildingUnits = _allBuildingUnitsByKey.Keys
                .Where(x => x == buildingUnitKey || buildingUnitKey.IsParentOf(x))
                .Aggregate(buildingUnits, (current, key) => current.Concat(GetRetiredUnitsByKey(key)));

            if (_readdressedKeys.ContainsKey(buildingUnitKey))
                buildingUnits = buildingUnits.Union(GetRetiredBuildingUnitsByKeyAndChildKeys(_readdressedKeys[buildingUnitKey]));

            return buildingUnits.ToList();
        }

        public IEnumerable<BuildingUnit> GetActiveBuildingUnitsByKeyAndChildKeys(BuildingUnitKey buildingUnitKey)
        {
            var buildingUnits = ActiveBuildingUnits.Select(unit =>
            {
                if (unit.BuildingUnitKey == buildingUnitKey || buildingUnitKey.IsParentOf(unit.BuildingUnitKey))
                    return unit;
                return null;
            }).Where(x => x != null);

            if (_readdressedKeys.ContainsKey(buildingUnitKey))
                buildingUnits = buildingUnits.Union(GetActiveBuildingUnitsByKeyAndChildKeys(_readdressedKeys[buildingUnitKey]));

            return buildingUnits;
        }

        public BuildingUnitId GetNextBuildingUnitIdFor(BuildingUnitKey buildingUnitKey, bool canBeDeleted = false)
        {
            if (HasActiveUnitByKey(buildingUnitKey))
                throw new InvalidOperationException("Cannot get next building unit id for active building unit");

            if (buildingUnitKey.HouseNumber.HasValue)
            {
                if ((_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) && _allBuildingUnitsByKey[buildingUnitKey].Any(x => x.IsRemoved) && !canBeDeleted))
                    throw new InvalidOperationException("Cannot get next building unit id for removed building unit");

                var version = 1;
                if (HasRetiredUnitByKey(buildingUnitKey))
                {
                    version = GetRetiredUnitsByKey(buildingUnitKey).Count() + 1;
                }

                if (canBeDeleted)
                {
                    version += _allBuildingUnits.Where(x => x.IsRemoved)
                        .Count(x => x.BuildingUnitKey == buildingUnitKey);
                }

                return BuildingUnitId.Create(buildingUnitKey, version);
            }
            else
            {
                var version = (_allBuildingUnitsByKey.ContainsKey(buildingUnitKey) ? _allBuildingUnitsByKey[buildingUnitKey].Count : 0) + 1;
                return BuildingUnitId.Create(buildingUnitKey, version);
            }
        }

        public bool IsAddressLinkedToCommonBuildingUnit(AddressId addressId)
        {
            return ActiveCommonBuildingUnit != null &&
                   ActiveCommonBuildingUnit.AddressIds.Contains(addressId);
        }

        public IEnumerable<BuildingUnitId> GetActiveBuildingUnitIdsToRetire()
        {
            var buildingUnitsToRetire = ActiveBuildingUnits
                .Where(x => x.Status == BuildingUnitStatus.Realized)
                .ToList();

            return buildingUnitsToRetire.Select(x => x.BuildingUnitId);
        }

        public IEnumerable<BuildingUnitId> GetActiveBuildingUnitIdsToNotRealized()
        {
            var buildingUnitsToRetire = ActiveBuildingUnits
                .Where(x => x.Status != BuildingUnitStatus.Realized)
                .ToList();

            return buildingUnitsToRetire.Select(x => x.BuildingUnitId);
        }

        public IEnumerable<BuildingUnitId> GetAllBuildingUnitIdsToRemove()
        {
            var retiredUnits = _allBuildingUnits
                .Where(_isRetiredPredicate)
                .GroupBy(x => x.BuildingUnitKey)
                .ToDictionary(x => x.Key, x => x.ToList());

            var ids = retiredUnits
                .SelectMany(x => x.Value)
                .Select(retiredBuildingUnit => retiredBuildingUnit.BuildingUnitId)
                .ToList();

            ids.AddRange(ActiveBuildingUnits.Select(activeBuildingUnit => activeBuildingUnit.BuildingUnitId));

            return ids;
        }

        public void Add(BuildingUnit buildingUnit)
        {
            _allBuildingUnits.Add(buildingUnit);

            if (!_allBuildingUnitsByKey.ContainsKey(buildingUnit.BuildingUnitKey))
                _allBuildingUnitsByKey.Add(buildingUnit.BuildingUnitKey, new List<BuildingUnit>());

            _allBuildingUnitsByKey[buildingUnit.BuildingUnitKey].Add(buildingUnit);
        }

        public void AddReaddress(BuildingUnitKey newBuildingUnitKey, BuildingUnitKey oldBuildingUnitKey)
        {
            if (_allBuildingUnitsByKey.ContainsKey(newBuildingUnitKey))
                throw new InvalidOperationException(
                    "Cannot add a new readdressing with key that is already a key for another unit");

            _readdressedKeys.Add(newBuildingUnitKey, oldBuildingUnitKey);
        }

        public void RouteToNonDeleted(object @event)
        {
            foreach (var buildingUnit in _allBuildingUnits.Where(x => !x.IsRemoved))
                buildingUnit.Route(@event);
        }

        public IEnumerable<BuildingUnit> GetAllBuildingUnitsWithoutPersistentLocalId()
        {
            return _allBuildingUnits.Where(x => x.PersistentLocalId == null);
        }
    }
}
