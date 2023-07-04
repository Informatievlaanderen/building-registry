namespace BuildingRegistry.Importer.Console.Crab.Subaddress
{
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;    
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;    

    internal static class ReaddressSubaddressImporter
    {
        public static IEnumerable<ImportReaddressingSubaddressFromCrab> GetCommands(
            List<int> subadresIds,
            int terrainObjectId,
            CrabTerrainObjectHouseNumberId key,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportReaddressingSubaddressFromCrab>();

            using (var crabEntities = crabEntitiesFactory())
            {
                var heradresseringSubadressen = HeradresseringQueries.GetHeradresseringSubadressen(subadresIds, crabEntities)
                    .Where(x => x.NieuwTerreinobject_HuisnummerId == key || x.OudTerreinobject_HuisnummerId == key);

                list.AddRange(GetCommandsFor(heradresseringSubadressen, new CrabTerrainObjectId(terrainObjectId)));
            }

            return list
                .OrderBy(x => x.BeginDate);
        }

        private static IEnumerable<ImportReaddressingSubaddressFromCrab> GetCommandsFor(
            IEnumerable<vwHeradresseringGR> heradresseringen,
            CrabTerrainObjectId terrainObjectId)
        {
            return
                heradresseringen
                    .Select(heradresseringGr =>
                    {
                        MapLogging.Log(".");

                        return new ImportReaddressingSubaddressFromCrab(
                            terrainObjectId,
                            new CrabReaddressingId(heradresseringGr.heradresseringId),
                            new ReaddressingBeginDate(new LocalDate(heradresseringGr.beginDatum.Value.Year, heradresseringGr.beginDatum.Value.Month, heradresseringGr.beginDatum.Value.Day)),
                            new CrabSubaddressId(heradresseringGr.oudAdresId),
                            new CrabAddressNature(heradresseringGr.oudAardAdres.ToString()),
                            new CrabTerrainObjectHouseNumberId(heradresseringGr.oudTerreinobject_HuisnummerId.Value),
                            new CrabSubaddressId(heradresseringGr.nieuwAdresId.Value),
                            new CrabAddressNature(heradresseringGr.nieuwAardAdres.ToString()),
                            new CrabTerrainObjectHouseNumberId(heradresseringGr.nieuwTerreinobject_HuisnummerId.Value)
                        );
                    });
        }

    }
}
