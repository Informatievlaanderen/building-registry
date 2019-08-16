namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System.Collections.Generic;
    using GeoAPI.Geometries;

    public interface IGrbBuildingParcel
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometry);
    }
}
