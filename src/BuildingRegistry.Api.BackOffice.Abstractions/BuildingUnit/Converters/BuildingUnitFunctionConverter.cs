namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;

    public static class BuildingUnitFunctionConverter
    {
        public static BuildingUnitFunction Map(this GebouweenheidFunctie f)
        {
            return f switch
            {
                GebouweenheidFunctie.GemeenschappelijkDeel => BuildingUnitFunction.Common,
                GebouweenheidFunctie.NietGekend => BuildingUnitFunction.Unknown,
                _ => throw new ArgumentException(nameof(f), f.ToString(), null)
            };
        }
    }
}
