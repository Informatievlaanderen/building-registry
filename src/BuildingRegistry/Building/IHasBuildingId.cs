namespace BuildingRegistry.Building
{
    using System;

    public interface IHasBuildingId
    {
        public Guid BuildingId { get; }
    }
}
