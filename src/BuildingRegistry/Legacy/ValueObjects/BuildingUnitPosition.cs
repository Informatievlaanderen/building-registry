namespace BuildingRegistry.Legacy
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class BuildingUnitPosition : ValueObject<BuildingUnitPosition>
    {
        public ExtendedWkbGeometry Geometry { get; }
        public BuildingUnitPositionGeometryMethod GeometryMethod { get; }

        public BuildingUnitPosition(
            [JsonProperty("geometry")] ExtendedWkbGeometry geometry,
            [JsonProperty("geometryMethod")] BuildingUnitPositionGeometryMethod geometryMethod)
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
