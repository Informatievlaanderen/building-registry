namespace BuildingRegistry.Building
{
    using System;

    public struct BuildingUnitStatus
    {
        public static readonly BuildingUnitStatus Planned = new BuildingUnitStatus("Planned");
        public static readonly BuildingUnitStatus Realized = new BuildingUnitStatus("Realized");
        public static readonly BuildingUnitStatus Retired = new BuildingUnitStatus("Retired");
        public static readonly BuildingUnitStatus NotRealized = new BuildingUnitStatus("NotRealized");

        public string Status { get; }

        private BuildingUnitStatus(string status) => Status = status;

        public static BuildingUnitStatus Parse(string status)
        {
            if (status != Planned.Status &&
               status != Realized.Status &&
               status != Retired.Status &&
               status != NotRealized.Status)
            {
                throw new NotImplementedException($"Cannot parse {status} to BuildingUnitStatus");
            }

            return new BuildingUnitStatus(status);
        }

        public static implicit operator string(BuildingUnitStatus status) => status.Status;
    }
}
