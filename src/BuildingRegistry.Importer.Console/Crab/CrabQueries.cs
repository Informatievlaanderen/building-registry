namespace BuildingRegistry.Importer.Console.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;

    public static class CrabQueries
    {
        private const string AardGebouw = "5";

        public static List<int> GetChangedGebouwIdsBetween(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory, int lastProcessedId = -1)
        {
            if (since == DateTime.MinValue)
            {
                using (var crabEntities = crabEntitiesFactory())
                {
                    var terreinObjects = crabEntities.tblTerreinObject.Where(terreinobject => terreinobject.aardTerreinObjectCode == AardGebouw);
                    var terreinObjectsHist = crabEntities.tblTerreinObject_hist.Where(terreinobject => terreinobject.aardTerreinObjectCode == AardGebouw);

                    return terreinObjects
                        .GroupBy(o => o.terreinObjectId).Select(terreinObject => new { terreinObject.Key, beginTijd = terreinObject.Min(tijden => tijden.beginTijd) })
                        .Concat(terreinObjectsHist.GroupBy(hist => hist.terreinObjectId).Select(terreinObjectHists => new { Key = terreinObjectHists.Key.Value, beginTijd = terreinObjectHists.Min(tijden => tijden.beginTijd.Value) }))
                        .GroupBy(x => x.Key)
                        .Select(x => new  { x.Key, beginTijd = x.Min(j => j.beginTijd) })
                        .Where(s => s.Key > lastProcessedId && s.beginTijd <= until)
                        .OrderBy(s => s.beginTijd)
                        .Select(s => s.Key)
                        .ToList();
                }
            }

            var tasks = new List<Task<List<int>>>
            {
                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var terreinObjectIds = new List<int>();
                        terreinObjectIds.AddRange(crabEntities
                            .tblTerreinObject
                            .Where(o => o.aardTerreinObjectCode == AardGebouw)
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until)
                            .Select(hnr => hnr.terreinObjectId)
                            .ToList());

                        terreinObjectIds.AddRange(crabEntities
                            .tblTerreinObject_hist
                            .Where(o => o.aardTerreinObjectCode == AardGebouw)
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove &&
                                                     crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.terreinObjectId.Value)
                            .ToList());

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var terreinObjectIds = new List<int>();
                        terreinObjectIds.AddRange(crabEntities
                            .tblGebouwgeometrie
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.terreinobjectid)
                            .ToList());

                        terreinObjectIds.AddRange(crabEntities
                            .tblGebouwgeometrie_hist
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.terreinobjectid.Value)
                            .ToList());

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var terreinObjectIds = new List<int>();
                        terreinObjectIds.AddRange(crabEntities
                            .tblGebouwstatus
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.terreinobjectid)
                            .ToList());

                        terreinObjectIds.AddRange(crabEntities
                            .tblGebouwstatus_hist
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.terreinobjectid.Value)
                            .ToList());

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var terreinObjectIds = new List<int>();
                        terreinObjectIds.AddRange(crabEntities
                            .tblTerreinObject_huisNummer
                            .Where(thn => thn.tblTerreinObject.aardTerreinObjectCode == AardGebouw)
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until)
                            .Select(hnr => hnr.terreinObjectId)
                            .ToList());

                        terreinObjectIds.AddRange(crabEntities
                            .tblTerreinObject_huisNummer_hist
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.terreinObjectId.Value)
                            .ToList());

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var huisnummerIDs = crabEntities
                            .tblHuisNummer
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until)
                            .Select(hnr => hnr.huisNummerId)
                            .ToList();

                        var terreinObjectIds = IterateSqlContains(huisnummerIDs, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(
                                        sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(
                                        crabEntities.tblTerreinObject_huisNummer_hist.Where(
                                                sa => idsInRange.Contains(sa.huisNummerId.Value))
                                            .Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var huisnummerIDsHist = crabEntities
                            .tblHuisNummer_hist
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.huisNummerId.Value)
                            .ToList();

                        terreinObjectIds.AddRange(IterateSqlContains(huisnummerIDsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var huisnummerIDs = crabEntities
                            .tblHuisnummerstatus
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.huisnummerid)
                            .ToList();

                        var terreinObjectIds = IterateSqlContains(huisnummerIDs, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var huisnummerIDsHist = crabEntities
                            .tblHuisnummerstatus_hist
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until ||
                                                 (crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.huisnummerid.Value)
                            .ToList();

                        terreinObjectIds.AddRange(IterateSqlContains(huisnummerIDsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var huisnummerIDs = crabEntities
                            .tblAdrespositie
                            .Where(pos => pos.aardAdres == "2")
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.adresid)
                            .ToList();

                        var terreinObjectIds = IterateSqlContains(huisnummerIDs, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var huisnummerIDsHist = crabEntities
                            .tblAdrespositie_hist
                            .Where(pos => pos.aardAdres == "2")
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until ||
                                                 (crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.adresid.Value)
                            .ToList();

                        terreinObjectIds.AddRange(IterateSqlContains(huisnummerIDsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var subadresIds = crabEntities
                            .tblSubAdres
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until)
                            .Select(hnr => hnr.huisNummerId)
                            .ToList();

                        var terreinObjectIds = IterateSqlContains(subadresIds, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var subadresIdsHist = crabEntities
                            .tblSubAdres_hist
                            .Where(crabRecord => crabRecord.beginTijd >= since && crabRecord.beginTijd < until ||
                                                 (crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.huisNummerId.Value)
                            .ToList();

                        terreinObjectIds.AddRange(IterateSqlContains(subadresIdsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var subadresIds = crabEntities
                            .tblSubadresstatus
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.tblSubAdres.huisNummerId)
                            .ToList();

                        var terreinObjectIds = IterateSqlContains(subadresIds, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var subadresIdsHist = crabEntities
                            .tblSubadresstatus_hist
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until ||
                                                 (crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.subadresid.Value)
                            .ToList();

                        var huisnummerIDs = IterateSqlContains(subadresIdsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblSubAdres.Where(sa => idsInRange.Contains(sa.subAdresId)).Select(sa => sa.huisNummerId)
                                    .Concat(crabEntities.tblSubAdres_hist.Where(sa => idsInRange.Contains(sa.subAdresId.Value)).Select(sa => sa.huisNummerId.Value))
                                    .ToList());
                        });

                        terreinObjectIds.AddRange(IterateSqlContains(huisnummerIDs, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),

                Task.Run(() =>
                {
                    using (var crabEntities = crabEntitiesFactory())
                    {
                        var subadresIds = crabEntities
                            .tblAdrespositie
                            .Where(pos => pos.aardAdres == "1")
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until)
                            .Select(hnr => hnr.adresid)
                            .ToList();

                        var huisnummerIds = IterateSqlContains(subadresIds, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblSubAdres.Where(sa => idsInRange.Contains(sa.subAdresId)).Select(sa => sa.huisNummerId)
                                    .Concat(crabEntities.tblSubAdres_hist.Where(sa => idsInRange.Contains(sa.subAdresId.Value)).Select(sa => sa.huisNummerId.Value))
                                    .ToList());
                        });

                        var terreinObjectIds = IterateSqlContains(huisnummerIds, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        });

                        var subadresIdsHist = crabEntities
                            .tblAdrespositie_hist
                            .Where(pos => pos.aardAdres == "1")
                            .Where(crabRecord => crabRecord.begintijd >= since && crabRecord.begintijd < until || (
                                                     crabRecord.eindBewerking == BewerkingCodes.Remove && crabRecord.eindTijd >= since && crabRecord.eindTijd < until))
                            .Select(hnr => hnr.adresid.Value)
                            .ToList();

                        var huisnummerIdsHist = IterateSqlContains(subadresIdsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblSubAdres.Where(sa => idsInRange.Contains(sa.subAdresId)).Select(sa => sa.huisNummerId)
                                    .Concat(crabEntities.tblSubAdres_hist.Where(sa => idsInRange.Contains(sa.subAdresId.Value)).Select(sa => sa.huisNummerId.Value))
                                    .ToList());
                        });

                        terreinObjectIds.AddRange(IterateSqlContains(huisnummerIdsHist, (idsInRange, filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId)).Select(sa => sa.terreinObjectId)
                                    .Concat(crabEntities.tblTerreinObject_huisNummer_hist.Where(sa => idsInRange.Contains(sa.huisNummerId.Value)).Select(sa => sa.terreinObjectId.Value))
                                    .ToList());
                        }));

                        return terreinObjectIds;
                    }
                }),
            };

            Task.WaitAll(tasks.ToArray());

            var allTerreinObjectIds = tasks.SelectMany(s => s.Result).ToList();
            var filteredTerreinobjectIDs = new List<int>();

            using (var crabEntities = crabEntitiesFactory())
            {
                filteredTerreinobjectIDs = IterateSqlContains(allTerreinObjectIds, (idsInRange, filteredIds) =>
                {
                    filteredIds.AddRange(crabEntities.tblTerreinObject.Where(
                            t => t.aardTerreinObjectCode == AardGebouw && idsInRange.Contains(t.terreinObjectId))
                        .Select(t => t.terreinObjectId));

                    filteredIds.AddRange(
                        crabEntities.tblTerreinObject_hist.Where(
                                t => t.aardTerreinObjectCode == AardGebouw && idsInRange.Contains(t.terreinObjectId.Value))
                            .Select(t => t.terreinObjectId.Value));
                });
            }

            return filteredTerreinobjectIDs.Distinct().ToList();
        }

        private static List<int> IterateSqlContains(IReadOnlyCollection<int> allIds, Action<List<int>, List<int>> addRangeAction)
        {
            var filteredIds = new List<int>();
            const int sqlContainsSize = 1000;
            for (var i = 0; i < Math.Ceiling(allIds.Count / (double)sqlContainsSize); i++)
            {
                var idsInThisRange = allIds
                    .Skip(i * sqlContainsSize)
                    .Take(Math.Min(sqlContainsSize, allIds.Count - i * sqlContainsSize))
                    .ToList();

                addRangeAction(idsInThisRange, filteredIds);
            }

            return filteredIds;
        }
    }
}
