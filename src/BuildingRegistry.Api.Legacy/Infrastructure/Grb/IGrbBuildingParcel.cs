namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System.Collections.Generic;

    public interface IGrbBuildingParcel
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometry);
    }
}
