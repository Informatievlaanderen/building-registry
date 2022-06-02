namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingUnitPosition : ValueObject<BuildingUnitPosition>
    {
        public ExtendedWkbGeometry Geometry { get; }
        public BuildingUnitPositionGeometryMethod GeometryMethod { get; }

        public BuildingUnitPosition(
            ExtendedWkbGeometry geometry,
            BuildingUnitPositionGeometryMethod geometryMethod)
        {
            Geometry = geometry;
            GeometryMethod = geometryMethod;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return Geometry;
            yield return GeometryMethod;
        }
    }
}
