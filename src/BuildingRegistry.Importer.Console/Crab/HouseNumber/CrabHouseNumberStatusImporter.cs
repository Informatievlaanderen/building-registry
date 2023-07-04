namespace BuildingRegistry.Importer.Console.Crab.HouseNumber
{
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;    
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class CrabHouseNumberStatusImporter
    {
        public static IEnumerable<ImportHouseNumberStatusFromCrab> GetCommands(
            ILookup<int, CrabTerrainObjectHouseNumberId> terrainObjectHouseNumberIdsByHouseNumber,
            int terreinObjectId, Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportHouseNumberStatusFromCrab>();
            using (var crabEntities = crabEntitiesFactory())
            {
                var allHouseNumbers = terrainObjectHouseNumberIdsByHouseNumber.Select(x => x.Key).ToList();

                var huisnummerstatusses =
                    AdresHuisnummerQueries.GetTblHuisnummerstatussenByHuisnummerIds(allHouseNumbers, crabEntities);
                var huisnummerstatussesHist =
                    AdresHuisnummerQueries.GetTblHuisnummerstatussenHistByHuisnummerIds(allHouseNumbers, crabEntities);

                var huisnummerStatusesByHuisnummer = huisnummerstatusses.GroupBy(x => x.huisnummerid).ToDictionary(x => x.Key, y => y.ToList());
                var huisnummerStatusesHistByHuisnummer = huisnummerstatussesHist.GroupBy(x => x.huisnummerid).ToDictionary(x => x.Key, y => y.ToList());

                //Duplicate per terrainobject housenumber relation
                foreach (var tobjHnrId in terrainObjectHouseNumberIdsByHouseNumber)
                {
                    foreach (var crabTerrainObjectHouseNumberId in tobjHnrId)
                    {
                        if (huisnummerStatusesByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandFromHuisnummerstatusses(huisnummerStatusesByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                        if (huisnummerStatusesHistByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandFromHuisnummerstatussesHist(huisnummerStatusesHistByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                    }
                }
            }

            return list
                .OrderBy(a => (Instant)a.Timestamp); //Return in correct order
        }

        private static IEnumerable<ImportHouseNumberStatusFromCrab> GetCommandFromHuisnummerstatussesHist(
            IEnumerable<tblHuisnummerstatus_hist> huisnummerstatussesHist,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return
                huisnummerstatussesHist
                    .Select(huisnummerstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportHouseNumberStatusFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            terrainObjectHouseNumberId,
                            new CrabHouseNumberStatusId(huisnummerstatus.huisnummerstatusid.Value),
                            new CrabHouseNumberId(huisnummerstatus.huisnummerid.Value),
                            ParseHuisnummerStatus(huisnummerstatus.Status),
                            new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(huisnummerstatus.begindatum?.ToCrabLocalDateTime(), huisnummerstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(huisnummerstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(huisnummerstatus.Operator),
                            CrabMappings.ParseBewerking(huisnummerstatus.Bewerking),
                            CrabMappings.ParseOrganisatie(huisnummerstatus.Organisatie)
                        );
                    });
        }

        private static IEnumerable<ImportHouseNumberStatusFromCrab> GetCommandFromHuisnummerstatusses(
            IEnumerable<tblHuisnummerstatus> huisnummerstatusses,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return
                huisnummerstatusses
                    .Select(huisnummerstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportHouseNumberStatusFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            terrainObjectHouseNumberId,
                            new CrabHouseNumberStatusId(huisnummerstatus.huisnummerstatusid),
                            new CrabHouseNumberId(huisnummerstatus.huisnummerid),
                            ParseHuisnummerStatus(huisnummerstatus.Status),
                            new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(huisnummerstatus.begindatum.ToCrabLocalDateTime(), huisnummerstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(huisnummerstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(huisnummerstatus.Operator),
                            CrabMappings.ParseBewerking(huisnummerstatus.Bewerking),
                            CrabMappings.ParseOrganisatie(huisnummerstatus.Organisatie)
                        );
                    });
        }

        private static CrabAddressStatus ParseHuisnummerStatus(CrabHuisnummerStatusEnum huisnummerStatus)
        {
            if (huisnummerStatus.Code == CrabHuisnummerStatusEnum.Gereserveerd.Code)
                return CrabAddressStatus.Reserved;
            if (huisnummerStatus.Code == CrabHuisnummerStatusEnum.Voorgesteld.Code)
                return CrabAddressStatus.Proposed;
            if (huisnummerStatus.Code == CrabHuisnummerStatusEnum.InGebruik.Code)
                return CrabAddressStatus.InUse;
            if (huisnummerStatus.Code == CrabHuisnummerStatusEnum.BuitenGebruik.Code)
                return CrabAddressStatus.OutOfUse;
            if (huisnummerStatus.Code == CrabHuisnummerStatusEnum.NietOfficieel.Code)
                return CrabAddressStatus.Unofficial;

            throw new ApplicationException("Onbekende huisnummer status");
        }
    }
}
