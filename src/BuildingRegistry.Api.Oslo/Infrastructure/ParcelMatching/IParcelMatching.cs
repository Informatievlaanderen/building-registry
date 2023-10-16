namespace BuildingRegistry.Api.Oslo.Infrastructure.ParcelMatching
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IParcelMatching
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes);
        IEnumerable<int> GetUnderlyingBuildings(Geometry parcelGeometry);
    }
}
