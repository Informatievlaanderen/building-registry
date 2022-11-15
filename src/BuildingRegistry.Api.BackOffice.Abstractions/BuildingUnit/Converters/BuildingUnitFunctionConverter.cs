namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Building;

    public static class BuildingUnitFunctionConverter
    {
        public static BuildingUnitFunction Map(this GebouweenheidFunctie f)
        {
            return f switch
            {
                GebouweenheidFunctie.NietGekend => BuildingUnitFunction.Unknown,
                GebouweenheidFunctie.Wonen => BuildingUnitFunction.Residential,
                GebouweenheidFunctie.Verblijfsrecreatie => BuildingUnitFunction.Lodging,
                GebouweenheidFunctie.DagrecreatieSport => BuildingUnitFunction.DayRecreationSport,
                GebouweenheidFunctie.LandTuinbouw => BuildingUnitFunction.AgricultureHorticulture,
                GebouweenheidFunctie.Detailhandel => BuildingUnitFunction.Retail,
                GebouweenheidFunctie.DancingRestaurantCafe => BuildingUnitFunction.DancingRestaurantCafe,
                GebouweenheidFunctie.KantoorDienstverleningVrijBeroep => BuildingUnitFunction.OfficeServicesLiberalProfession,
                GebouweenheidFunctie.IndustrieBedrijvigheid => BuildingUnitFunction.IndustryBusiness,
                GebouweenheidFunctie.GemeenschapsOpenbareNutsvoorziening => BuildingUnitFunction.CommunityPublicUtility,
                GebouweenheidFunctie.MilitaireFunctie => BuildingUnitFunction.MilitaryFunction,
                _ => throw new ArgumentException(nameof(f), f.ToString(), null)
            };
        }
    }
}
