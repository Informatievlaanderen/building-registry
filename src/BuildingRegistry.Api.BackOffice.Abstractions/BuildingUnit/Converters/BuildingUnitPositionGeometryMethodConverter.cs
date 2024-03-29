namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Building;

    public static class BuildingUnitPositionGeometryMethodConverter
    {
        public static BuildingUnitPositionGeometryMethod Map(this PositieGeometrieMethode methode)
        {
            switch (methode)
            {
                case PositieGeometrieMethode.AangeduidDoorBeheerder:
                    return BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
                case PositieGeometrieMethode.AfgeleidVanObject:
                    return BuildingUnitPositionGeometryMethod.DerivedFromObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(methode), methode, null);
            }
        }
    }
}
