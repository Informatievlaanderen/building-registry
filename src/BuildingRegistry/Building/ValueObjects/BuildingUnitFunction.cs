namespace BuildingRegistry.Building
{
    using System;

    public struct BuildingUnitFunction
    {
        public static readonly BuildingUnitFunction Common = new BuildingUnitFunction("Common");
        public static readonly BuildingUnitFunction Unknown = new BuildingUnitFunction("Unknown");
        public static readonly BuildingUnitFunction Residential = new BuildingUnitFunction("Residential");
        public static readonly BuildingUnitFunction Lodging = new BuildingUnitFunction("Lodging");
        public static readonly BuildingUnitFunction DayRecreationSport = new BuildingUnitFunction("DayRecreationSport");
        public static readonly BuildingUnitFunction AgricultureHorticulture = new BuildingUnitFunction("AgricultureHorticulture");
        public static readonly BuildingUnitFunction Retail = new BuildingUnitFunction("Retail");
        public static readonly BuildingUnitFunction DancingRestaurantCafe = new BuildingUnitFunction("DancingRestaurantCafe");
        public static readonly BuildingUnitFunction OfficeServicesLiberalProfession = new BuildingUnitFunction("OfficeServicesLiberalProfession");
        public static readonly BuildingUnitFunction IndustryBusiness = new BuildingUnitFunction("IndustryBusiness");
        public static readonly BuildingUnitFunction CommunityPublicUtility = new BuildingUnitFunction("CommunityPublicUtility");
        public static readonly BuildingUnitFunction MilitaryFunction = new BuildingUnitFunction("MilitaryFunction");

        public string Function { get; }

        private BuildingUnitFunction(string function) => Function = function;

        public static BuildingUnitFunction Parse(string function)
        {
            if (function != Common.Function &&
                function != Unknown.Function &&
                function != Residential.Function &&
                function != Lodging.Function &&
                function != DayRecreationSport.Function &&
                function != AgricultureHorticulture.Function &&
                function != Retail.Function &&
                function != DancingRestaurantCafe.Function &&
                function != OfficeServicesLiberalProfession.Function &&
                function != IndustryBusiness.Function &&
                function != CommunityPublicUtility.Function &&
                function != MilitaryFunction.Function)
            {
                throw new NotImplementedException($"Cannot parse {function} to BuildingUnitFunction");
            }

            return new BuildingUnitFunction(function);
        }

        public static implicit operator string(BuildingUnitFunction function) => function.Function;
    }
}
