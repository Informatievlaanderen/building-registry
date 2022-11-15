namespace BuildingRegistry.Api.Legacy.Abstractions.Converters
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;
    using Legacy = BuildingRegistry.Legacy;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus ConvertFromBuildingUnitStatus(this Legacy.BuildingUnitStatus status)
        {
            if (status == Legacy.BuildingUnitStatus.NotRealized)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (status == Legacy.BuildingUnitStatus.Planned)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (status == Legacy.BuildingUnitStatus.Realized)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (status == Legacy.BuildingUnitStatus.Retired)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Legacy.BuildingUnitStatus ConvertFromGebouweenheidStatus(this GebouweenheidStatus status)
        {
            if (status == GebouweenheidStatus.NietGerealiseerd)
            {
                return Legacy.BuildingUnitStatus.NotRealized;
            }

            if (status == GebouweenheidStatus.Gepland)
            {
                return Legacy.BuildingUnitStatus.Planned;
            }

            if (status == GebouweenheidStatus.Gerealiseerd)
            {
                return Legacy.BuildingUnitStatus.Realized;
            }

            if (status == GebouweenheidStatus.Gehistoreerd)
            {
                return Legacy.BuildingUnitStatus.Retired;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static GebouweenheidStatus Map(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static PositieGeometrieMethode ConvertFromBuildingUnitGeometryMethod(this Legacy.BuildingUnitPositionGeometryMethod method)
        {
            if (method == Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }

        public static PositieGeometrieMethode Map(this BuildingUnitPositionGeometryMethod method)
        {
            if (method == BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie? ConvertFromBuildingUnitFunction(this Legacy.BuildingUnitFunction? function)
        {
            if (function == null)
            {
                return null;
            }

            if (function == Legacy.BuildingUnitFunction.Unknown)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            if (function == Legacy.BuildingUnitFunction.Common)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        public static GebouweenheidFunctie? Map(this BuildingUnitFunction function)
        {
            var dictionary = new Dictionary<BuildingUnitFunction, GebouweenheidFunctie>
            {
                { BuildingUnitFunction.Common, GebouweenheidFunctie.GemeenschappelijkDeel },
                { BuildingUnitFunction.Unknown, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.Residential, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.Lodging, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.DayRecreationSport, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.AgricultureHorticulture, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.Retail, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.DancingRestaurantCafe, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.OfficeServicesLiberalProfession, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.IndustryBusiness, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.CommunityPublicUtility, GebouweenheidFunctie.NietGekend },
                { BuildingUnitFunction.MilitaryFunction, GebouweenheidFunctie.NietGekend }
            };

            return dictionary.ContainsKey(function)
                ? dictionary[function]
                : throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }
}
