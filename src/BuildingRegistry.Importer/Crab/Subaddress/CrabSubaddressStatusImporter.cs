using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
using Aiv.Vbr.CentraalBeheer.Crab.Entity;
using Aiv.Vbr.CrabModel;
using Be.Vlaanderen.Basisregisters.Crab;
using BuildingRegistry.Building.Commands.Crab;
using BuildingRegistry.ValueObjects.Crab;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingRegistry.Importer.Crab.Subaddress
{
    public static class CrabSubaddressStatusImporter
    {
        public static IEnumerable<ImportSubaddressStatusFromCrab> GetCommands(List<int> subadresIds,
            int terreinObjectId,
            int terrainObjectHuisnummerId,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportSubaddressStatusFromCrab>();

            using (var crabEntities = crabEntitiesFactory())
            {
                var subadresStatusses =
                    AdresSubadresQueries.GetTblSubadresstatussenBySubadresIds(subadresIds, crabEntities);
                var subadresStatussesHist =
                    AdresSubadresQueries.GetTblSubadresstatussenHistBySubadresIds(subadresIds, crabEntities);

                list.AddRange(GetCommandsFor(subadresStatusses, terreinObjectId, terrainObjectHuisnummerId));
                list.AddRange(GetCommandsFor(subadresStatussesHist, terreinObjectId, terrainObjectHuisnummerId));
            }

            return list
                .OrderBy(a => (Instant)a.Timestamp); //Return in correct order
        }

        private static IEnumerable<ImportSubaddressStatusFromCrab> GetCommandsFor(
            IEnumerable<tblSubadresstatus_hist> subadresStatussesHist,
            int terreinObjectId,
            int terrainObjectHouseNumberId)
        {
            return subadresStatussesHist
                .Select(
                    subadresstatusHist =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressStatusFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                            new CrabSubaddressStatusId(subadresstatusHist.subadresstatusid.Value),
                            new CrabSubaddressId(subadresstatusHist.subadresid.Value),
                            ParseSubaddressStatus(subadresstatusHist.Status),
                            new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(subadresstatusHist.begindatum?.ToCrabLocalDateTime(), subadresstatusHist.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subadresstatusHist.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subadresstatusHist.Operator),
                            CrabMappings.ParseBewerking(subadresstatusHist.Bewerking),
                            CrabMappings.ParseOrganisatie(subadresstatusHist.Organisatie));
                    });
        }

        private static IEnumerable<ImportSubaddressStatusFromCrab> GetCommandsFor(
            IEnumerable<tblSubadresstatus> subadresStatuses,
            int terreinObjectId,
            int terrainObjectHouseNumberId)
        {
            return subadresStatuses
                .Select(
                    subadresstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressStatusFromCrab(
                            new CrabTerrainObjectId(terreinObjectId),
                            new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                            new CrabSubaddressStatusId(subadresstatus.subadresstatusid),
                            new CrabSubaddressId(subadresstatus.subadresid),
                            ParseSubaddressStatus(subadresstatus.Status),
                            new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(subadresstatus.begindatum.ToCrabLocalDateTime(), subadresstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subadresstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subadresstatus.Operator),
                            CrabMappings.ParseBewerking(subadresstatus.Bewerking),
                            CrabMappings.ParseOrganisatie(subadresstatus.Organisatie));
                    });
        }

        private static CrabAddressStatus ParseSubaddressStatus(CrabSubadresStatusEnum subadresStatus)
        {
            if (subadresStatus.Code == CrabSubadresStatusEnum.Gereserveerd.Code)
                return CrabAddressStatus.Reserved;
            if (subadresStatus.Code == CrabSubadresStatusEnum.Voorgesteld.Code)
                return CrabAddressStatus.Proposed;
            if (subadresStatus.Code == CrabSubadresStatusEnum.InGebruik.Code)
                return CrabAddressStatus.InUse;
            if (subadresStatus.Code == CrabSubadresStatusEnum.BuitenGebruik.Code)
                return CrabAddressStatus.OutOfUse;
            if (subadresStatus.Code == CrabSubadresStatusEnum.NietOfficieel.Code)
                return CrabAddressStatus.Unofficial;

            throw new ApplicationException($"Onbekende subadres status {subadresStatus}");
        }
    }
}
