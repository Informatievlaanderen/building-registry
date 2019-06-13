namespace BuildingRegistry.ValueObjects
{
    using System;

    public struct BuildingUnitPositionGeometryMethod
    {
        public static readonly BuildingUnitPositionGeometryMethod AppointedByAdministrator = new BuildingUnitPositionGeometryMethod("AppointedByAdministrator");
        public static readonly BuildingUnitPositionGeometryMethod DerivedFromObject = new BuildingUnitPositionGeometryMethod("DerivedFromObject");

        public string GeometryMethod { get; }

        private BuildingUnitPositionGeometryMethod(string geometryMethod)
        {
            GeometryMethod = geometryMethod;
        }

        public static BuildingUnitPositionGeometryMethod Parse(string geometryMethod)
        {
            if (geometryMethod != AppointedByAdministrator.GeometryMethod &&
                geometryMethod != DerivedFromObject.GeometryMethod)
                throw new NotImplementedException($"Cannot parse {geometryMethod} to BuildingUnitPositionGeometryMethod");

            return new BuildingUnitPositionGeometryMethod(geometryMethod);
        }

        public static implicit operator string(BuildingUnitPositionGeometryMethod positionGeometryMethod)
        {
            return positionGeometryMethod.GeometryMethod;
        }
    }
}
