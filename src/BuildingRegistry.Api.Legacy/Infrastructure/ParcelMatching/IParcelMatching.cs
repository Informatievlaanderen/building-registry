namespace BuildingRegistry.Api.Legacy.Infrastructure.ParcelMatching
{
    using System.Collections.Generic;

    public interface IParcelMatching
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes);
    }
}
