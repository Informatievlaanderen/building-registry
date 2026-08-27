namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;

    public interface IParcels
    {
        /// <summary>
        /// Parcels whose geometry, in <paramref name="matchingSrid"/>, intersects the building's bounding
        /// box. <see cref="ParcelData.Geometry"/> is returned in that same system, so the caller can compare
        /// it directly against a building geometry it has brought to it. See ADR 0006.
        /// </summary>
        Task<IEnumerable<ParcelData>> GetUnderlyingParcelsUnderBoundingBox(Geometry buildingGeometry, int matchingSrid);

        /// <summary>
        /// Whether any parcel is still missing its Lambert 2008 geometry, which would make it invisible to
        /// matching done in that system. Backs <see cref="Lambert2008MatchingReadiness"/>.
        /// </summary>
        Task<bool> HasIncompleteLambert2008Geometry();
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
