namespace BuildingRegistry.Importer.Console
{
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
    using BuildingRegistry.Building.Commands.Crab;
    using Crab;
    using Crab.HouseNumber;
    using Crab.Subaddress;
    using ValueObjects;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;

    public class CommandGenerator : ICommandGenerator<int>
    {
        private readonly string _vbrConnectionString;
        private readonly Func<CRABEntities> _crabEntitiesFactory;
        private readonly bool _singleUpdate;
        private readonly Lazy<ILookup<int, AssignPersistentLocalIdForCrabTerrainObjectId>> _osloIdCommands;
        private readonly Lazy<List<int>> _terrainObjectIdsWhichNeedNewPersistentId;

        public CommandGenerator(string vbrConnectionString, Func<CRABEntities> crabEntitiesFactory, bool singleUpdate = false)
        {
            _vbrConnectionString = vbrConnectionString;
            _crabEntitiesFactory = crabEntitiesFactory;
            _singleUpdate = singleUpdate;
            _osloIdCommands = new Lazy<ILookup<int, AssignPersistentLocalIdForCrabTerrainObjectId>>(() => GetOsloCommandsToPost().ToLookup(x => (int)x.TerrainObjectId, x => x));
            _terrainObjectIdsWhichNeedNewPersistentId = new Lazy<List<int>>(() => GetCorruptedTerrainObjectIdsWhichNeedNewPersistentLocalId());
        }

        private List<int> GetCorruptedTerrainObjectIdsWhichNeedNewPersistentLocalId()
        {
            //Get the terrain object id's of buildings with more than one duplicate of same building unit
            //These will be processed differently.
            using (var connection = new SqlConnection(_vbrConnectionString))
            {
                return connection.Query<int>(
                    "select distinct TerreinobjectID from (" +
                    "select TerreinobjectID, [index] from crab.gebouweenheidmapping " +
                    "group by TerreinobjectID, [index] " +
                    "having count(*) > 2" +
                    ") a").ToList();
            }
        }

        private IEnumerable<AssignPersistentLocalIdForCrabTerrainObjectId> GetOsloCommandsToPost()
        {
            using (var connection = new SqlConnection(_vbrConnectionString))
            {
                var buildingUnitOsloIdsByTerrainObjectId = connection.Query<Crab2VbrTerrainObjectBuildingUnitMapping>(
                        "SELECT o.ObjectID, m.TerreinObjectId, m.TerreinobjectHuisnummerId, m.SubadresId, m.[Index], m.MappingCreatedTimestamp " +
                        "FROM crab.Gebouweenheidmapping m " +
                        "INNER JOIN crab.GebouweenheidObjectID o ON m.gebouweenheididinternal = o.gebouweenheididinternal " +
                        "ORDER BY m.gebouweenheididinternal", commandTimeout: (60 * 5))
                    .GroupBy(x => x.TerreinObjectId)
                    .ToDictionary(x => x.Key, y => y
                    .Select(mapping => new AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
                        mapping.TerreinObjectHuisnummerId.HasValue ? new CrabTerrainObjectHouseNumberId(mapping.TerreinObjectHuisnummerId.Value) : null,
                        mapping.SubadresId.HasValue ? new CrabSubaddressId(mapping.SubadresId.Value) : null,
                        mapping.Index,
                        new PersistentLocalId(mapping.ObjectId),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(mapping.MappingCreatedTimestamp)))));

                var crab2VbrTerrainObjectMappings = connection.Query<Crab2VbrTerrainObjectMapping>(
                    "SELECT o.ObjectID, m.CrabTerreinobjectID, m.MappingCreatedTimestamp " +
                    "FROM crab.GebouwMapping m " +
                    "INNER JOIN crab.GebouwObjectID o ON m.GebouwIDInternal = o.GebouwIDInternal " +
                    "ORDER BY m.CrabTerreinObjectID", commandTimeout: (60 * 5));

                return crab2VbrTerrainObjectMappings
                    .Select(mapping => new AssignPersistentLocalIdForCrabTerrainObjectId(
                        new CrabTerrainObjectId(mapping.CrabTerreinObjectId),
                        new PersistentLocalId(mapping.ObjectId),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(mapping.MappingCreatedTimestamp)),
                        buildingUnitOsloIdsByTerrainObjectId.ContainsKey(mapping.CrabTerreinObjectId)
                            ? buildingUnitOsloIdsByTerrainObjectId[mapping.CrabTerreinObjectId].ToList()
                            : new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>()));
            }
        }

        private IEnumerable<AssignPersistentLocalIdForCrabTerrainObjectId> GetOsloCommandsToPost(int terreinObjectId)
        {
            using (var connection = new SqlConnection(_vbrConnectionString))
            {
                var buildingUnitOsloIdsByTerrainObjectId = connection.Query<Crab2VbrTerrainObjectBuildingUnitMapping>(
                        "SELECT o.ObjectID, m.TerreinObjectId, m.TerreinobjectHuisnummerId, m.SubadresId, m.[Index], m.MappingCreatedTimestamp " +
                        "FROM crab.Gebouweenheidmapping m " +
                        "INNER JOIN crab.GebouweenheidObjectID o ON m.gebouweenheididinternal = o.gebouweenheididinternal " +
                        "WHERE m.TerreinObjectID = @terreinObjectId " +
                        "ORDER BY m.gebouweenheididinternal", new { terreinObjectId }, commandTimeout: (60 * 5))
                    .GroupBy(x => x.TerreinObjectId)
                    .ToDictionary(x => x.Key, y => y
                    .Select(mapping => new AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
                        mapping.TerreinObjectHuisnummerId.HasValue ? new CrabTerrainObjectHouseNumberId(mapping.TerreinObjectHuisnummerId.Value) : null,
                        mapping.SubadresId.HasValue ? new CrabSubaddressId(mapping.SubadresId.Value) : null,
                        mapping.Index,
                        new PersistentLocalId(mapping.ObjectId),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(mapping.MappingCreatedTimestamp)))));

                var crab2VbrTerrainObjectMappings = connection.Query<Crab2VbrTerrainObjectMapping>(
                    "SELECT o.ObjectID, m.CrabTerreinobjectID, m.MappingCreatedTimestamp " +
                    "FROM crab.GebouwMapping m " +
                    "INNER JOIN crab.GebouwObjectID o ON m.GebouwIDInternal = o.GebouwIDInternal " +
                    "WHERE m.CrabTerreinObjectID = @terreinObjectId " +
                    "ORDER BY m.CrabTerreinObjectID", new { terreinObjectId }, commandTimeout: (60 * 5));

                return crab2VbrTerrainObjectMappings
                    .Select(mapping => new AssignPersistentLocalIdForCrabTerrainObjectId(
                        new CrabTerrainObjectId(mapping.CrabTerreinObjectId),
                        new PersistentLocalId(mapping.ObjectId),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(mapping.MappingCreatedTimestamp)),
                        buildingUnitOsloIdsByTerrainObjectId.ContainsKey(mapping.CrabTerreinObjectId)
                            ? buildingUnitOsloIdsByTerrainObjectId[mapping.CrabTerreinObjectId].ToList()
                            : new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>()));
            }
        }

        public IEnumerable<int> GetChangedKeys(DateTime @from, DateTime until)
        {
            return CrabQueries.GetChangedGebouwIdsBetween(from, until, _crabEntitiesFactory).Distinct();
        }

        public IEnumerable<dynamic> GenerateInitCommandsFor(int key, DateTime from, DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(ImportMode.Init, key, from, until);

            if (_terrainObjectIdsWhichNeedNewPersistentId.Value.Contains(key))
                crabCommands.Add(new RequestPersistentLocalIdsForCrabTerrainObjectId(new CrabTerrainObjectId(key)));
            else
                crabCommands.Add(!_singleUpdate
                    ? _osloIdCommands.Value[key].Single()
                    : GetOsloCommandsToPost(key).Single());

            return crabCommands;
        }

        public IEnumerable<dynamic> GenerateUpdateCommandsFor(int key, DateTime @from, DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(ImportMode.Update, key, from, until);

            crabCommands.Add(new RequestPersistentLocalIdsForCrabTerrainObjectId(new CrabTerrainObjectId(key)));

            return crabCommands;
        }

        protected List<dynamic> CreateCommandsInOrder(
            ImportMode importMode,
            int terreinobjectId,
            DateTime from,
            DateTime until)
        {
            var importTerrainObjectCommands = new List<ImportTerrainObjectFromCrab>();
            var importBuildingStatusCommands = new List<ImportBuildingStatusFromCrab>();
            var importBuildingGeometryCommands = new List<ImportBuildingGeometryFromCrab>();
            var importTerrainObjectHouseNumberCommands = new List<ImportTerrainObjectHouseNumberFromCrab>();

            var importHouseNumberStatusCommands = new List<ImportHouseNumberStatusFromCrab>();
            var importHouseNumberPositionCommands = new List<ImportHouseNumberPositionFromCrab>();
            var importHouseNumberReaddressingCommands = new List<ImportReaddressingHouseNumberFromCrab>();

            var importSubaddressCommands = new List<ImportSubaddressFromCrab>();
            var importSubaddressStatusCommands = new List<ImportSubaddressStatusFromCrab>();
            var importSubaddressPositionCommands = new List<ImportSubaddressPositionFromCrab>();
            var importSubaddressReaddressingCommands = new List<ImportReaddressingSubaddressFromCrab>();

            ILookup<int, CrabTerrainObjectHouseNumberId> huisnummerIds;
            using (var crabEntities = _crabEntitiesFactory())
            {
                var tblTerreinObjectByTerreinObjectId = TerreinObjectQueries.GetTblTerreinObjectByTerreinObjectId(terreinobjectId, crabEntities);
                if (tblTerreinObjectByTerreinObjectId != null)
                    importTerrainObjectCommands.Add(TerrainObjectImporter.GetCommandsFor(tblTerreinObjectByTerreinObjectId));

                importTerrainObjectCommands.AddRange(TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblTerreinObjectHistByTerreinObjectId(terreinobjectId, crabEntities)));

                importBuildingStatusCommands.AddRange(TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblGebouwStatussenByTerreinObjectId(terreinobjectId, crabEntities)));
                importBuildingStatusCommands.AddRange(TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblGebouwStatussenHistByTerreinObjectId(terreinobjectId, crabEntities)));

                importBuildingGeometryCommands.AddRange(TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblGebouwGeometrieenByTerreinObjectId(terreinobjectId, crabEntities)));
                importBuildingGeometryCommands.AddRange(TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblGebouwGeometrieenHistByTerreinObjectId(terreinobjectId, crabEntities)));

                var importTerrainObjectHouseNumberFromCrab = TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblTerreinObjectHuisNummersByTerreinObjectId(terreinobjectId, crabEntities)).ToList();
                var importTerrainObjectHouseNumberFromCrabHist = TerrainObjectImporter.GetCommandsFor(TerreinObjectQueries.GetTblTerreinObjectHuisNummersHistByTerreinObjectId(terreinobjectId, crabEntities)).ToList();

                huisnummerIds = importTerrainObjectHouseNumberFromCrab.Select(x =>
                        new { terrainObjectHnrId = x.TerrainObjectHouseNumberId, houseNumberId = (int)x.HouseNumberId })
                    .Union(importTerrainObjectHouseNumberFromCrabHist.Select(x =>
                        new { terrainObjectHnrId = x.TerrainObjectHouseNumberId, houseNumberId = (int)x.HouseNumberId }))
                    .ToLookup(x => x.houseNumberId, y => y.terrainObjectHnrId);

                importTerrainObjectHouseNumberCommands.AddRange(importTerrainObjectHouseNumberFromCrab);
                importTerrainObjectHouseNumberCommands.AddRange(importTerrainObjectHouseNumberFromCrabHist);

                try
                {
                    importTerrainObjectHouseNumberCommands =
                        CorrectRetiredRelations(importTerrainObjectHouseNumberCommands);
                }
                catch
                {
                    throw new InvalidOperationException($"{terreinobjectId} terrain object housenumber relations could not be corrected");
                }
            }

            importHouseNumberStatusCommands.AddRange(CrabHouseNumberStatusImporter.GetCommands(huisnummerIds, terreinobjectId, _crabEntitiesFactory));
            importHouseNumberPositionCommands.AddRange(CrabHouseNumberPositionImporter.GetCommands(huisnummerIds, terreinobjectId, _crabEntitiesFactory));
            importHouseNumberReaddressingCommands.AddRange(ReaddressHouseNumberImporter.GetCommands(huisnummerIds, terreinobjectId, _crabEntitiesFactory));

            var importSubaddressesFromCrab = CrabSubaddressImporter.GetCommands(huisnummerIds, terreinobjectId, _crabEntitiesFactory).ToList();
            importSubaddressCommands.AddRange(importSubaddressesFromCrab);

            var subadresIdsByTerrainObjectHouseNumberId = importSubaddressesFromCrab.GroupBy(x => x.TerrainObjectHouseNumberId).ToList();

            foreach (var subaddressIdsByTerrainObjectHnrIds in subadresIdsByTerrainObjectHouseNumberId)
            {
                var subadresIds = subaddressIdsByTerrainObjectHnrIds.Select(x => (int)x.SubaddressId).ToList();
                importSubaddressStatusCommands.AddRange(CrabSubaddressStatusImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, _crabEntitiesFactory));
                importSubaddressPositionCommands.AddRange(CrabSubaddressPositionImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, _crabEntitiesFactory));
                importSubaddressReaddressingCommands.AddRange(ReaddressSubaddressImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, _crabEntitiesFactory));
            }

            var readdressHouseNumberCommands = importHouseNumberReaddressingCommands
                .GroupBy(x => x.CreateCommandId())
                .Select(x => x.First());

            var readdressSubaddressCommands = importSubaddressReaddressingCommands
                .GroupBy(x => x.CreateCommandId())
                .Select(x => x.First());

            var allTerrainObjectCommands = importTerrainObjectCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 0, $"TerreinObjectId {x.TerrainObjectId}"))
                .Concat(importTerrainObjectHouseNumberCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 2, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importBuildingStatusCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 1, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importBuildingGeometryCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 0, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importHouseNumberStatusCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -9, 1, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importHouseNumberPositionCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -9, 3, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importSubaddressCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -9, 2, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importSubaddressStatusCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -8, 0, $"TerreinObjectId {x.TerrainObjectId}")))
                .Concat(importSubaddressPositionCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -8, 1, $"TerreinObjectId {x.TerrainObjectId}")))
                .ToList();

            var allCommands = allTerrainObjectCommands
                .Where(x => x.Item1.Timestamp > from.ToCrabInstant() && x.Item1.Timestamp <= until.ToCrabInstant())
                .ToList();

            if (importMode == ImportMode.Update)
            {
                var houseNumbersForUpdate = importTerrainObjectHouseNumberCommands
                    .Where(x => x.Timestamp > from.ToCrabInstant() && x.Timestamp <= until.ToCrabInstant())
                    .Select(x => x.HouseNumberId).ToList();

                if (houseNumbersForUpdate.Any())
                {
                    var houseNumbersBeforeUpdate = importTerrainObjectHouseNumberCommands
                        .Where(x => x.Timestamp <= from.ToCrabInstant())
                        .Select(x => x.HouseNumberId).ToList();

                    var newHouseNumbers = houseNumbersForUpdate.Except(houseNumbersBeforeUpdate);

                    foreach (var newHouseNumber in newHouseNumbers)
                    {
                        allCommands = allCommands.Concat(allTerrainObjectCommands
                                .Except(allCommands)
                                .Where(x =>
                                    (x.Item1 is ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab && importHouseNumberStatusFromCrab.HouseNumberId == newHouseNumber) ||
                                    (x.Item1 is ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab && importHouseNumberPositionFromCrab.HouseNumberId == newHouseNumber)))
                                .ToList();

                        var allNewSubaddressIds = importSubaddressCommands.Where(subaddressFromCrab => subaddressFromCrab.HouseNumberId == newHouseNumber).Select(x => x.SubaddressId);
                        foreach (var newSubaddressId in allNewSubaddressIds)
                        {
                            allCommands = allCommands.Concat(allTerrainObjectCommands
                                .Except(allCommands)
                                .Where(x =>
                                    (x.Item1 is ImportSubaddressFromCrab importSubaddressFromCrab && importSubaddressFromCrab.SubaddressId == newSubaddressId) ||
                                    (x.Item1 is ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab && importSubaddressStatusFromCrab.SubaddressId == newSubaddressId) ||
                                    (x.Item1 is ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab && importSubaddressPositionFromCrab.SubaddressId == newSubaddressId)))
                                .ToList();
                        }
                    }
                }
            }

            allCommands = allCommands
                .OrderBy(x => x.Item1.Timestamp)
                .ThenBy(x => x.Item2)
                .ThenBy(x => x.Item3)
                .ToList();

            ImportTerrainObjectFromCrab initialTerrainObjectCommand = null;
            var commands = new List<dynamic>();
            if (importTerrainObjectCommands.Any(x => x.Modification == CrabModification.Insert))
            {
                initialTerrainObjectCommand = importTerrainObjectCommands
                    .OrderBy(x => (Instant)x.Timestamp)
                    .FirstOrDefault();

                commands.Add(initialTerrainObjectCommand);
            }

            //send readdress commands (not filtered by timestamp) always first => they can anticipate on new address being added
            commands.AddRange(readdressHouseNumberCommands);
            commands.AddRange(readdressSubaddressCommands);

            foreach (var command in allCommands)
                if (!command.Item1.Equals(initialTerrainObjectCommand))
                    commands.Add(command.Item1);

            return commands;
        }

        private List<ImportTerrainObjectHouseNumberFromCrab> CorrectRetiredRelations(IEnumerable<ImportTerrainObjectHouseNumberFromCrab> importTerrainObjectHouseNumberCommands)
        {
            //if(!importTerrainObjectHouseNumberCommands.Any())
            //    return;

            var orderedCommands = importTerrainObjectHouseNumberCommands.OrderBy(x => x.Timestamp).ToList();
            var activeHouseNumberByRelation = new Dictionary<int, int>();

            var newCommands = new List<ImportTerrainObjectHouseNumberFromCrab>();
            var skipCommands = new List<ImportTerrainObjectHouseNumberFromCrab>();

            foreach (var importTerrainObjectHouseNumberFromCrab in orderedCommands)
            {
                if(skipCommands.Contains(importTerrainObjectHouseNumberFromCrab))
                    continue;

                var crabTerrainObjectHouseNumberId = importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId;
                var houseNumberId = importTerrainObjectHouseNumberFromCrab.HouseNumberId;

                if (activeHouseNumberByRelation.ContainsValue(houseNumberId))
                {
                    var terrainObjectHouseNrId = activeHouseNumberByRelation.Single(x => x.Value == houseNumberId).Key;
                    if (terrainObjectHouseNrId != crabTerrainObjectHouseNumberId &&
                        importTerrainObjectHouseNumberFromCrab.Modification != CrabModification.Delete &&
                        !importTerrainObjectHouseNumberFromCrab.Lifetime.EndDateTime.HasValue)
                    {
                        var nextCommandForCurrentTerrainObjectHouseNumberFromCrab = orderedCommands.First(x =>
                            x.TerrainObjectHouseNumberId == terrainObjectHouseNrId &&
                            (Instant)x.Timestamp > importTerrainObjectHouseNumberFromCrab.Timestamp);

                        var newTimestamp = new CrabTimestamp(((Instant) importTerrainObjectHouseNumberFromCrab.Timestamp).Minus(Duration.FromSeconds(1)));

                        activeHouseNumberByRelation.Remove(nextCommandForCurrentTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId);

                        skipCommands.Add(nextCommandForCurrentTerrainObjectHouseNumberFromCrab);
                        newCommands.Add(new ImportTerrainObjectHouseNumberFromCrab(
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.TerrainObjectId,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.HouseNumberId,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.Lifetime,
                            newTimestamp,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.Operator,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.Modification,
                            nextCommandForCurrentTerrainObjectHouseNumberFromCrab.Organisation));
                    }
                }

                if (!activeHouseNumberByRelation.ContainsKey(crabTerrainObjectHouseNumberId))
                    activeHouseNumberByRelation.Add(crabTerrainObjectHouseNumberId, houseNumberId);
                else if (activeHouseNumberByRelation[crabTerrainObjectHouseNumberId] != houseNumberId)
                    activeHouseNumberByRelation[crabTerrainObjectHouseNumberId] = houseNumberId;

                if (activeHouseNumberByRelation.ContainsKey(crabTerrainObjectHouseNumberId) && (
                    importTerrainObjectHouseNumberFromCrab.Modification == CrabModification.Delete || importTerrainObjectHouseNumberFromCrab.Lifetime.EndDateTime.HasValue))
                    activeHouseNumberByRelation.Remove(crabTerrainObjectHouseNumberId);

                newCommands.Add(importTerrainObjectHouseNumberFromCrab);
            }

            return newCommands;
        }

        public string Name => GetType().FullName;
    }

    internal class Crab2VbrTerrainObjectMapping
    {
        public int ObjectId { get; set; }
        public int CrabTerreinObjectId { get; set; }
        public DateTimeOffset MappingCreatedTimestamp { get; set; }
    }

    internal class Crab2VbrTerrainObjectBuildingUnitMapping
    {
        public int ObjectId { get; set; }
        public int TerreinObjectId { get; set; }
        public int? TerreinObjectHuisnummerId { get; set; }
        public int? SubadresId { get; set; }
        public int Index { get; set; }
        public DateTimeOffset MappingCreatedTimestamp { get; set; }
    }
}
