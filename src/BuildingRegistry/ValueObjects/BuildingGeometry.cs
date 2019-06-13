namespace BuildingRegistry.ValueObjects
{
    using NetTopologySuite.IO;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingGeometry : ValueObject<BuildingGeometry>
    {
        private readonly WKBReader _wkbReader = new WKBReader { HandleSRID = true };

        public ExtendedWkbGeometry Geometry { get; }
        public BuildingGeometryMethod Method { get; }

        public BuildingGeometry(ExtendedWkbGeometry geometry, BuildingGeometryMethod method)
        {
            Geometry = geometry;
            Method = method;
        }

        public bool Contains(ExtendedWkbGeometry geometry)
        {
            var buildingUnitGeometry = _wkbReader.Read(geometry);
            return _wkbReader.Read(Geometry).Contains(buildingUnitGeometry);
        }

        public ExtendedWkbGeometry Center =>
            ExtendedWkbGeometry.CreateEWkb(_wkbReader.Read(Geometry).Centroid.AsBinary());

        protected override IEnumerable<object> Reflect()
        {
            yield return Geometry;
            yield return Method;
        }
    }
}
