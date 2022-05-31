namespace BuildingRegistry.Building
{
    public readonly struct BuildingUnitKeyType
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
}
