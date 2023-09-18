namespace BuildingRegistry
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Building;
    using NetTopologySuite.Geometries;

    public interface IParcelMatching
    {
        Task<IEnumerable<ParcelData>> GetUnderlyingParcels(Geometry geometry);
    }
}
