using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
using Aiv.Vbr.CentraalBeheer.Crab.Entity;
using Aiv.Vbr.Common;
using Be.Vlaanderen.Basisregisters.Crab;
using BuildingRegistry.Building.Commands.Crab;
using BuildingRegistry.ValueObjects;
using BuildingRegistry.ValueObjects.Crab;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

namespace BuildingRegistry.Importer.Crab.Subaddress
{
    public static class CrabSubaddressPositionImporter
    {
        public static IEnumerable<ImportSubaddressPositionFromCrab> GetCommands(
            List<int> subadresIds,
            int terreinObjectId,
            int terreinObjectHuisnummerId,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportSubaddressPositionFromCrab>();

            using (var crabEntities = crabEntitiesFactory())
            {
                var subadresPosities =
                    AdresSubadresQueries.GetTblAdrespositiesBySubadresIds(subadresIds, crabEntities);
                var subadresPositiesHist =
                    AdresSubadresQueries.GetTblAdrespositiesHistBySubadresIds(subadresIds, crabEntities);

                list.AddRange(GetCommandsFor(subadresPosities, terreinObjectId, terreinObjectHuisnummerId));
                list.AddRange(GetCommandsFor(subadresPositiesHist, terreinObjectId, terreinObjectHuisnummerId));
            }

            return list
                .OrderBy(a => (Instant)a.Timestamp); //Return in correct order
        }

        private static IEnumerable<ImportSubaddressPositionFromCrab> GetCommandsFor(
            IEnumerable<tblAdrespositie_hist> huisnummerPositiesHist,
            int terreinObjectId,
            int terreinObjectHuisnummerId)
        {
            return
                huisnummerPositiesHist
                    .Select(
                        s =>
                        {
                            MapLogging.Log(".");

                            return new ImportSubaddressPositionFromCrab(
                                new CrabTerrainObjectId(terreinObjectId),
                                new CrabTerrainObjectHouseNumberId(terreinObjectHuisnummerId),
                                new CrabAddressPositionId(s.adrespositieid.Value),
                                new CrabSubaddressId(s.adresid.Value),
                                new WkbGeometry(DbGeometry.FromBinary(s.Geometrie.WKB, Constants.Lambert72Srid).AsBinary()),
                                new CrabAddressNature(s.aardAdres),
                                CrabMappings.ParseHerkomstAdresPositie(s.HerkomstAdrespositie),
                                new CrabLifetime(s.beginDatum?.ToCrabLocalDateTime(), s.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(s.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(s.Operator),
                                CrabMappings.ParseBewerking(s.Bewerking),
                                CrabMappings.ParseOrganisatie(s.Organisatie));
                        });
        }

        private static IEnumerable<ImportSubaddressPositionFromCrab> GetCommandsFor(
            IEnumerable<tblAdrespositie> huisnummerPosities,
            int terreinObjectId,
            int terreinObjectHuisnummerId)
        {
            return
                huisnummerPosities
                    .Select(
                        adrespositie =>
                        {
                            MapLogging.Log(".");

                            return new ImportSubaddressPositionFromCrab(
                                new CrabTerrainObjectId(terreinObjectId),
                                new CrabTerrainObjectHouseNumberId(terreinObjectHuisnummerId),
                                new CrabAddressPositionId(adrespositie.adrespositieid),
                                new CrabSubaddressId(adrespositie.adresid),
                                new WkbGeometry(DbGeometry.FromBinary(adrespositie.Geometrie.WKB, Constants.Lambert72Srid).AsBinary()),
                                new CrabAddressNature(adrespositie.aardAdres),
                                CrabMappings.ParseHerkomstAdresPositie(adrespositie.HerkomstAdrespositie),
                                new CrabLifetime(adrespositie.beginDatum.ToCrabLocalDateTime(), adrespositie.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(adrespositie.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(adrespositie.Operator),
                                CrabMappings.ParseBewerking(adrespositie.Bewerking),
                                CrabMappings.ParseOrganisatie(adrespositie.Organisatie));
                        });
        }
    }
}
