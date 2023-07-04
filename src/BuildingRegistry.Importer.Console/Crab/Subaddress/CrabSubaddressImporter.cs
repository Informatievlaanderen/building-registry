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

    public static class CrabSubaddressImporter
    {
        public static IEnumerable<ImportSubaddressFromCrab> GetCommands(
            ILookup<int, CrabTerrainObjectHouseNumberId> terrainObjectHouseNumberIdByHouseNumber,
            int terreinObjectId,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportSubaddressFromCrab>();
            var allHouseNumbers = terrainObjectHouseNumberIdByHouseNumber.Select(x => x.Key).ToList();
            using (var crabEntities = crabEntitiesFactory())
            {
                var subaddresses =
                    AdresSubadresQueries.GetTblSubAdressenByHuisnummerIds(allHouseNumbers, crabEntities);
                var subaddressesHist =
                    AdresSubadresQueries.GetTblSubAdressenHistByHuisnummerIds(allHouseNumbers, crabEntities);

                var subaddressesByHuisnummer = subaddresses.GroupBy(x => x.huisNummerId).ToDictionary(x => x.Key, y => y.ToList());
                var subaddressesHistByHuisnummer = subaddressesHist.GroupBy(x => x.huisNummerId.Value).ToDictionary(x => x.Key, y => y.ToList());

                foreach (var tobjHnrId in terrainObjectHouseNumberIdByHouseNumber)
                {
                    foreach (var crabTerrainObjectHouseNumberId in tobjHnrId)
                    {
                        if (subaddressesByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandFor(subaddressesByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                        if (subaddressesHistByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandFor(subaddressesHistByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                    }
                }
            }

            return list
                .OrderBy(a => (Instant)a.Timestamp); //Return in correct order
        }

        private static IEnumerable<ImportSubaddressFromCrab> GetCommandFor(
            IEnumerable<tblSubAdres_hist> subadresHist,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return subadresHist
                .Select(
                    subaddress =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            terrainObjectHouseNumberId,
                            new CrabSubaddressId(subaddress.subAdresId.Value),
                            new CrabHouseNumberId(subaddress.huisNummerId.Value),
                            new BoxNumber(subaddress.subAdres),
                            new CrabBoxNumberType(subaddress.aardSubAdresCode),
                            new CrabLifetime(subaddress.beginDatum?.ToCrabLocalDateTime(), subaddress.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subaddress.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subaddress.Operator),
                            CrabMappings.ParseBewerking(subaddress.Bewerking),
                            CrabMappings.ParseOrganisatie(subaddress.Organisatie));
                    });
        }

        private static IEnumerable<ImportSubaddressFromCrab> GetCommandFor(
            IEnumerable<tblSubAdres> subaddresses,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return subaddresses
                .Select(
                    subaddress =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            terrainObjectHouseNumberId,
                            new CrabSubaddressId(subaddress.subAdresId),
                            new CrabHouseNumberId(subaddress.huisNummerId),
                            new BoxNumber(subaddress.subAdres),
                            new CrabBoxNumberType(subaddress.aardSubAdresCode),
                            new CrabLifetime(subaddress.beginDatum.ToCrabLocalDateTime(), subaddress.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subaddress.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subaddress.Operator),
                            CrabMappings.ParseBewerking(subaddress.Bewerking),
                            CrabMappings.ParseOrganisatie(subaddress.Organisatie));
                    });
        }
    }
}
