namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;

    public interface IParcels
    {
        Task<IEnumerable<ParcelData>> GetUnderlyingParcelsUnderBoundingBox(Geometry buildingGeometry);
    }

    public class ParcelData
    {
        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public string Status { get; set; }
        public Geometry Geometry { get; set; }
        public List<AddressPersistentLocalId> Addresses { get; set; }

        public ParcelData(Guid parcelId, string caPaKey, Geometry geometry, string parcelStatus, List<AddressPersistentLocalId> addresses)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            Geometry = geometry;
            Status = parcelStatus;
            Addresses = addresses;
        }
    }
}
