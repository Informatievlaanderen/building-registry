namespace BuildingRegistry.Building
{
    using System;

    public struct BuildingGeometryMethod
    {
        public static BuildingGeometryMethod Outlined = new BuildingGeometryMethod("Outlined");
        public static BuildingGeometryMethod MeasuredByGrb = new BuildingGeometryMethod("MeasuredByGrb");

        public string Value { get; }

        private BuildingGeometryMethod(string value) => Value = value;

        public static implicit operator string(BuildingGeometryMethod value) => value.Value;

        public static BuildingGeometryMethod Parse(string status)
        {
            if (status != Outlined
                && status != MeasuredByGrb)
            {
                throw new NotImplementedException($"Cannot parse {status} to BuildingGeometryMethod");
            }

            return new BuildingGeometryMethod(status);
        }
    }
}
