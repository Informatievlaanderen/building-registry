namespace BuildingRegistry.Api.Oslo.Infrastructure.ParcelMatching
{
    using System.Collections.Generic;

    public interface IParcelMatching
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes);
    }
}
