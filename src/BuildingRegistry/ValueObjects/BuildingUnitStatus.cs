namespace BuildingRegistry.ValueObjects
{
    using System;

    public struct BuildingUnitStatus
    {
        public static BuildingUnitStatus Planned = new BuildingUnitStatus("Planned");
        public static BuildingUnitStatus Realized = new BuildingUnitStatus("Realized");
        public static BuildingUnitStatus Retired = new BuildingUnitStatus("Retired");
        public static BuildingUnitStatus NotRealized = new BuildingUnitStatus("NotRealized");

        public string Status { get; }

        private BuildingUnitStatus(string status) => Status = status;

        public static BuildingUnitStatus? Parse(string status)
        {
            if (string.IsNullOrEmpty(status))
                return null;

            if(status != Planned.Status &&
               status != Realized.Status &&
               status != Retired.Status &&
               status != NotRealized.Status)
                throw new NotImplementedException($"Cannot parse {status} to BuildingUnitStatus");

            return new BuildingUnitStatus(status);
        }

        public static implicit operator string(BuildingUnitStatus status) => status.Status;
    }
}
