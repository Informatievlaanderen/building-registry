using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
using Aiv.Vbr.CentraalBeheer.Crab.Entity;
using Be.Vlaanderen.Basisregisters.Crab;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
using BuildingRegistry.Building.Commands.Crab;
using BuildingRegistry.Importer.Crab;
using BuildingRegistry.Importer.Crab.HouseNumber;
using BuildingRegistry.Importer.Crab.Subaddress;
using BuildingRegistry.ValueObjects;
using Dapper;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;

namespace BuildingRegistry.Importer
{
    public class CommandGenerator : ICommandGenerator<int>
    {
        private readonly string _vbrConnectionString;
        private readonly bool _singleUpdate;
        private readonly Lazy<ILookup<int, AssignPersistentLocalIdForCrabTerrainObjectId>> _osloIdCommands;

        public CommandGenerator(string vbrConnectionString, bool singleUpdate = false)
        {
            _vbrConnectionString = vbrConnectionString;
            _singleUpdate = singleUpdate;
            _osloIdCommands = new Lazy<ILookup<int, AssignPersistentLocalIdForCrabTerrainObjectId>>(() => GetOsloCommandsToPost().ToLookup(x => (int)x.TerrainObjectId, x => x));
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
            return CrabQueries.GetChangedGebouwIdsBetween(from, until).Distinct();
        }

        public IEnumerable<dynamic> GenerateInitCommandsFor(int key, DateTime from, DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(ImportMode.Init, key, from, until);

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
            using (var crabEntities = new CRABEntities())
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
            }

            CRABEntities CrabEntitiesFactory() => new CRABEntities();
            importHouseNumberStatusCommands.AddRange(CrabHouseNumberStatusImporter.GetCommands(huisnummerIds, terreinobjectId, CrabEntitiesFactory));
            importHouseNumberPositionCommands.AddRange(CrabHouseNumberPositionImporter.GetCommands(huisnummerIds, terreinobjectId, CrabEntitiesFactory));
            importHouseNumberReaddressingCommands.AddRange(ReaddressHouseNumberImporter.GetCommands(huisnummerIds, terreinobjectId, CrabEntitiesFactory));

            var importSubaddressesFromCrab = CrabSubaddressImporter.GetCommands(huisnummerIds, terreinobjectId, CrabEntitiesFactory).ToList();
            importSubaddressCommands.AddRange(importSubaddressesFromCrab);

            var subadresIdsByTerrainObjectHouseNumberId = importSubaddressesFromCrab.GroupBy(x => x.TerrainObjectHouseNumberId).ToList();

            foreach (var subaddressIdsByTerrainObjectHnrIds in subadresIdsByTerrainObjectHouseNumberId)
            {
                var subadresIds = subaddressIdsByTerrainObjectHnrIds.Select(x => (int)x.SubaddressId).ToList();
                importSubaddressStatusCommands.AddRange(CrabSubaddressStatusImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, CrabEntitiesFactory));
                importSubaddressPositionCommands.AddRange(CrabSubaddressPositionImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, CrabEntitiesFactory));
                importSubaddressReaddressingCommands.AddRange(ReaddressSubaddressImporter.GetCommands(subadresIds, terreinobjectId, subaddressIdsByTerrainObjectHnrIds.Key, CrabEntitiesFactory));
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
            if (importMode == ImportMode.Init)
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
                if (importMode == ImportMode.Update || !command.Item1.Equals(initialTerrainObjectCommand))
                    commands.Add(command.Item1);

            return commands;
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
