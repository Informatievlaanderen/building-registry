using System;
using System.Collections.Generic;
using System.Linq;
using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
using Aiv.Vbr.CentraalBeheer.Crab.Entity;
using Be.Vlaanderen.Basisregisters.Crab;
using BuildingRegistry.Building.Commands.Crab;
using BuildingRegistry.ValueObjects;
using BuildingRegistry.ValueObjects.Crab;
using NodaTime;

namespace BuildingRegistry.Importer.Crab.HouseNumber
{
    internal static class ReaddressHouseNumberImporter
    {
        public static IEnumerable<ImportReaddressingHouseNumberFromCrab> GetCommands(
            ILookup<int, CrabTerrainObjectHouseNumberId> terrainObjectHouseNumberIdsByHouseNumber,
            int terreinObjectId,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportReaddressingHouseNumberFromCrab>();
            using (var crabEntities = crabEntitiesFactory())
            {
                var allHouseNumbers = terrainObjectHouseNumberIdsByHouseNumber.Select(x => x.Key).ToList();

                var huisnummerHeradresseringen = HeradresseringQueries.GetHeradresseringenHuisnummers(allHouseNumbers, crabEntities);
                var huisnummerHeradresseringenByHuisnummer = huisnummerHeradresseringen.GroupBy(x => x.oudAdresId).ToDictionary(x => x.Key, y => y.ToList());

                //Duplicate per terrainobject housenumber relation
                foreach (var tobjHnrId in terrainObjectHouseNumberIdsByHouseNumber)
                {
                    foreach (var crabTerrainObjectHouseNumberId in tobjHnrId)
                    {
                        if (huisnummerHeradresseringenByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandsFor(huisnummerHeradresseringenByHuisnummer[tobjHnrId.Key], new CrabTerrainObjectId(terreinObjectId))
                                .Where(x=> x.NewTerrainObjectHouseNumberId == crabTerrainObjectHouseNumberId || x.OldTerrainObjectHouseNumberId == crabTerrainObjectHouseNumberId));
                    }
                }
            }

            return list
                .OrderBy(x => x.BeginDate);
        }

        private static IEnumerable<ImportReaddressingHouseNumberFromCrab> GetCommandsFor(
            IEnumerable<vwHeradresseringGR> heradresseringen,
            CrabTerrainObjectId terrainObjectId)
        {
            return
                heradresseringen
                    .Select(heradresseringGr =>
                    {
                        MapLogging.Log(".");

                        return new ImportReaddressingHouseNumberFromCrab(
                            terrainObjectId,
                            new CrabReaddressingId(heradresseringGr.heradresseringId),
                            new ReaddressingBeginDate(new LocalDate(heradresseringGr.beginDatum.Value.Year, heradresseringGr.beginDatum.Value.Month, heradresseringGr.beginDatum.Value.Day)),
                            new CrabHouseNumberId(heradresseringGr.oudAdresId),
                            new CrabAddressNature(heradresseringGr.oudAardAdres.ToString()),
                            new CrabTerrainObjectHouseNumberId(heradresseringGr.oudTerreinobject_HuisnummerId.Value),
                            new CrabHouseNumberId(heradresseringGr.nieuwAdresId.Value),
                            new CrabAddressNature(heradresseringGr.nieuwAardAdres.ToString()),
                            new CrabTerrainObjectHouseNumberId(heradresseringGr.nieuwTerreinobject_HuisnummerId.Value)
                        );
                    });
        }
    }
}
