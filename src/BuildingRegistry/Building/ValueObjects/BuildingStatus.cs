namespace BuildingRegistry.Building
{
    using System;

    public struct BuildingStatus
    {
        public static readonly BuildingStatus Planned = new BuildingStatus("Planned");
        public static readonly BuildingStatus UnderConstruction = new BuildingStatus("UnderConstruction");
        public static readonly BuildingStatus Realized = new BuildingStatus("Realized");
        public static readonly BuildingStatus Retired = new BuildingStatus("Retired");
        public static readonly BuildingStatus NotRealized = new BuildingStatus("NotRealized");

        public string Value { get; }

        private BuildingStatus(string value) => Value = value;

        public static implicit operator string(BuildingStatus status) => status.Value;

        public static BuildingStatus Parse(string status)
        {
            if (status != Planned
                && status != UnderConstruction
                && status != Realized
                && status != Retired
                && status != NotRealized)
            {
                throw new NotImplementedException($"Cannot parse {status} to BuildingStatus");
            }

            return new BuildingStatus(status);
        }
    }
}
