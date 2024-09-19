namespace BuildingRegistry.Projections.Legacy
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IBuildingMatching
    {
        IEnumerable<int> GetUnderlyingBuildings(Geometry parcelGeometry);
    }
}
