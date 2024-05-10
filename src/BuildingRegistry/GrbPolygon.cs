namespace BuildingRegistry
{
    using NetTopologySuite.Geometries;

    /// <summary>
    /// Some GRB polygons are Invalid according to NTS Spec (and the world). But GRB (using SQL validation) decided it is valid.
    /// In order not to invalidate and correct (change) the polygon from the source we use this class.
    /// https://github.com/NetTopologySuite/NetTopologySuite/issues/259
    /// </summary>
    public class GrbPolygon : Polygon
    {
        public override bool IsValid => GeometryValidator.IsValid(this);

        public GrbPolygon(Polygon polygon) : base(polygon.Shell, polygon.Holes, polygon.Factory) { }
    }
}
