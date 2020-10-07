namespace BuildingRegistry
{
    using NetTopologySuite.Geometries;

    public static class GeometryValidator
    {
        public static bool IsValid(Geometry geometry)
        {
            var validOp =
                new NetTopologySuite.Operation.Valid.IsValidOp(geometry)
                {
                    IsSelfTouchingRingFormingHoleValid = true
                };

            return validOp.IsValid;
        }

        //public static Geometry? MakeValid(Geometry? geometry)
        //{
        //    //https://groups.google.com/g/nettopologysuite/c/Cp3DGU2T_ng
        //    if (geometry != null && !geometry.IsValid && IsValid(geometry))
        //       return geometry.Buffer(0);

        //    return geometry;
        //}
    }
}
