namespace BuildingRegistry.ValueObjects
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    //public class BuildingUnitKey : GuidValueObject<BuildingUnitKey>
    //{
    //    private static readonly Guid Namespace = new Guid("5b836de0-7eb7-491e-99da-9b51162711d8");

    //    public BuildingUnitKey(Guid buildingUnitId) : base(buildingUnitId) { }

    //    public static BuildingUnitKey Create(CrabTerrainObjectId terrainObjectId)
    //    {
    //        return new BuildingUnitKey(Deterministic.Create(Namespace, BuildDeterministicString(terrainObjectId)));
    //    }

    //    public static BuildingUnitKey Create(
    //        CrabTerrainObjectId terrainObjectId,
    //        CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
    //    {
    //        return new BuildingUnitKey(Deterministic.Create(Namespace, BuildDeterministicString(terrainObjectId, terrainObjectHouseNumberId)));
    //    }

    //    public static BuildingUnitKey Create(
    //        CrabTerrainObjectId terrainObjectId,
    //        CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
    //        CrabSubaddressId subaddressId)
    //    {
    //        return new BuildingUnitKey(Deterministic.Create(Namespace, BuildDeterministicString(terrainObjectId, terrainObjectHouseNumberId, subaddressId)));
    //    }

    //    private static string BuildDeterministicString(
    //        CrabTerrainObjectId terrainObjectId,
    //        CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId = null,
    //        CrabSubaddressId subaddressId = null)
    //    {
    //        var stringBuilder = new StringBuilder($"TerrainObjectId-{terrainObjectId}");

    //        if (terrainObjectHouseNumberId != null)
    //            stringBuilder.Append($"TerrainObjectHouseNumberId-{terrainObjectHouseNumberId}");

    //        if (subaddressId != null)
    //            stringBuilder.Append($"SubaddressId-{subaddressId}");

    //        return stringBuilder.ToString();
    //    }
    //}

    public struct BuildingUnitKeyType
    {
        public int Building { get; }

        public int? HouseNumber { get; }

        public int? Subaddress { get; }

        public BuildingUnitKeyType(int building, int? houseNumber = null, int? subaddress = null)
        {
            Building = building;
            HouseNumber = houseNumber;
            Subaddress = subaddress;
        }

        public override string ToString() => $"{Building}_{HouseNumber}_{Subaddress}";
    }

    public class BuildingUnitKey : StructDataTypeValueObject<BuildingUnitKey, BuildingUnitKeyType>
    {
        public BuildingUnitKey([JsonProperty("value")] BuildingUnitKeyType value) : base(value)
        {
        }

        public static BuildingUnitKey Create(CrabTerrainObjectId terrainObjectId)
            => new BuildingUnitKey(new BuildingUnitKeyType(terrainObjectId));

        public static BuildingUnitKey Create(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
            => new BuildingUnitKey(new BuildingUnitKeyType(terrainObjectId, terrainObjectHouseNumberId));

        public static BuildingUnitKey Create(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressId subaddressId)
            => new BuildingUnitKey(new BuildingUnitKeyType(terrainObjectId, terrainObjectHouseNumberId, subaddressId));

        public int Building => Value.Building;
        public int? HouseNumber => Value.HouseNumber;
        public int? Subaddress => Value.Subaddress;

        public BuildingUnitKey ToHouseNumberKey()
        {
            if (!HouseNumber.HasValue)
                throw new InvalidOperationException("Cannot convert to a HouseNumber key if HouseNumber has no value");

            return Create(new CrabTerrainObjectId(Building), new CrabTerrainObjectHouseNumberId(HouseNumber.Value));
        }

        public BuildingUnitKey ToBuildingKey() => Create(new CrabTerrainObjectId(Building));

        public bool IsParentOf(BuildingUnitKey childKey)
        {
            if (Subaddress.HasValue)
                return false;

            if (childKey.Subaddress.HasValue)
                return Building == childKey.Building && HouseNumber == childKey.HouseNumber;

            if (HouseNumber.HasValue)
                return false;

            return Building == childKey.Building;
        }

        public BuildingUnitKey CreateParentKey()
        {
            if (Subaddress.HasValue)
                return Create(new CrabTerrainObjectId(Building), new CrabTerrainObjectHouseNumberId(HouseNumber.Value));

            if (HouseNumber.HasValue)
                return Create(new CrabTerrainObjectId(Building));

            throw new InvalidOperationException("Cannot create parent key for top hierachical level");
        }
    }
}
