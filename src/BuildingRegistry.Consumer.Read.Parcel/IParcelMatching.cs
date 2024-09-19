namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System.Collections.Generic;

    public interface IParcelMatching
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes);
    }
}
