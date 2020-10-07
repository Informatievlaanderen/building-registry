namespace BuildingRegistry
{
    using NetTopologySuite.Geometries;

    public class GrbPolygon : Polygon
    {
        public override bool IsValid => GeometryValidator.IsValid(this);

        public GrbPolygon(LinearRing shell, LinearRing[] holes) : base(shell, holes) { }

        public GrbPolygon(LinearRing shell, LinearRing[] holes, GeometryFactory factory) : base(shell, holes, factory) { }

        public GrbPolygon(LinearRing shell, GeometryFactory factory) : base(shell, factory) { }

        public GrbPolygon(LinearRing shell) : base(shell) { }

        public GrbPolygon(Polygon polygon) : base(polygon.Shell, polygon.Holes, polygon.Factory) { }
    }
}
