namespace BuildingRegistry.Importer.Console.Crab.HouseNumber
{
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.Common;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Building.Commands.Crab;
    using ValueObjects;
    using BuildingRegistry.ValueObjects.Crab;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Linq;

    internal static class CrabHouseNumberPositionImporter
    {
        public static IEnumerable<ImportHouseNumberPositionFromCrab> GetCommands(
            ILookup<int, CrabTerrainObjectHouseNumberId> terrainObjectHouseNumberIdByHouseNumber,
            int terreinObjectId,
            Func<CRABEntities> crabEntitiesFactory)
        {
            var list = new List<ImportHouseNumberPositionFromCrab>();
            var allHouseNumbers = terrainObjectHouseNumberIdByHouseNumber.Select(x => x.Key).ToList();
            using (var crabEntities = crabEntitiesFactory())
            {
                var huisnummerPosities =
                    AdresHuisnummerQueries.GetTblAdresPositiesByHuisnummerIds(allHouseNumbers, crabEntities);
                var huisnummerHist =
                    AdresHuisnummerQueries.GetTblAdresPositiesHistByHuisnummerIds(allHouseNumbers, crabEntities);

                var huisnummerPositionsByHuisnummer = huisnummerPosities.GroupBy(x => x.adresid).ToDictionary(x => x.Key, y => y.ToList());
                var huisnummerPositionsHistByHuisnummer = huisnummerHist.GroupBy(x => x.adresid).ToDictionary(x => x.Key, y => y.ToList());

                //Duplicate per terrainobject housenumber relation
                foreach (var tobjHnrId in terrainObjectHouseNumberIdByHouseNumber)
                {
                    foreach (var crabTerrainObjectHouseNumberId in tobjHnrId)
                    {
                        if (huisnummerPositionsByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandsFor(huisnummerPositionsByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                        if (huisnummerPositionsHistByHuisnummer.ContainsKey(tobjHnrId.Key))
                            list.AddRange(GetCommandsFor(huisnummerPositionsHistByHuisnummer[tobjHnrId.Key], crabTerrainObjectHouseNumberId, terreinObjectId));
                    }
                }
            }

            return list
                .OrderBy(a => (Instant)a.Timestamp); //Return in correct order
        }

        private static IEnumerable<ImportHouseNumberPositionFromCrab> GetCommandsFor(
            IEnumerable<tblAdrespositie_hist> huisnummerPositiesHist,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return
                huisnummerPositiesHist
                    .Select(
                        s =>
                        {
                            MapLogging.Log(".");

                            return new ImportHouseNumberPositionFromCrab(
                                new CrabTerrainObjectId(terreinObjectId),
                                terrainObjectHouseNumberId,
                                new CrabAddressPositionId(s.adrespositieid.Value),
                                new CrabHouseNumberId(s.adresid.Value),
                                new WkbGeometry(DbGeometry.FromBinary(s.Geometrie.WKB, Constants.Lambert72Srid).AsBinary()),
                                new CrabAddressNature(s.aardAdres),
                                CrabMappings.ParseHerkomstAdresPositie(s.HerkomstAdrespositie),
                                new CrabLifetime(s.beginDatum?.ToCrabLocalDateTime(), s.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(s.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(s.Operator),
                                CrabMappings.ParseBewerking(s.Bewerking),
                                CrabMappings.ParseOrganisatie(s.Organisatie)
                            );
                        });
        }

        private static IEnumerable<ImportHouseNumberPositionFromCrab> GetCommandsFor(
            IEnumerable<tblAdrespositie> huisnummerPosities,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            int terreinObjectId)
        {
            return
                huisnummerPosities
                    .Select(
                        adrespositie =>
                        {
                            MapLogging.Log(".");

                            return new ImportHouseNumberPositionFromCrab(
                                new CrabTerrainObjectId(terreinObjectId),
                                terrainObjectHouseNumberId,
                                new CrabAddressPositionId(adrespositie.adrespositieid),
                                new CrabHouseNumberId(adrespositie.adresid),
                                new WkbGeometry(DbGeometry.FromBinary(adrespositie.Geometrie.WKB, Constants.Lambert72Srid).AsBinary()),
                                new CrabAddressNature(adrespositie.aardAdres),
                                CrabMappings.ParseHerkomstAdresPositie(adrespositie.HerkomstAdrespositie),
                                new CrabLifetime(adrespositie.beginDatum.ToCrabLocalDateTime(), adrespositie.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(adrespositie.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(adrespositie.Operator),
                                CrabMappings.ParseBewerking(adrespositie.Bewerking),
                                CrabMappings.ParseOrganisatie(adrespositie.Organisatie)
                            );
                        });
        }
    }
}
