using Aiv.Vbr.CentraalBeheer.Crab.Entity;
using Be.Vlaanderen.Basisregisters.Crab;
using BuildingRegistry.Building.Commands.Crab;
using BuildingRegistry.ValueObjects;
using BuildingRegistry.ValueObjects.Crab;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingRegistry.Importer.Crab
{
    public static class TerrainObjectImporter
    {
        public static IEnumerable<ImportTerrainObjectHouseNumberFromCrab> GetCommandsFor(IEnumerable<tblTerreinObject_huisNummer_hist> terreinObjectHuisNummersHist)
        {
            return terreinObjectHuisNummersHist
                .Select(
                    terreinObjectHuisNummer =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectHouseNumberFromCrab(
                            new CrabTerrainObjectHouseNumberId(terreinObjectHuisNummer.terreinObject_huisNummer_Id.Value),
                            new CrabTerrainObjectId(terreinObjectHuisNummer.terreinObjectId.Value),
                            new CrabHouseNumberId(terreinObjectHuisNummer.huisNummerId.Value),
                            new CrabLifetime(terreinObjectHuisNummer.beginDatum?.ToCrabLocalDateTime(), terreinObjectHuisNummer.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(NodaHelpers.ToCrabInstant((DateTime)terreinObjectHuisNummer.CrabTimestamp)),
                            new CrabOperator(terreinObjectHuisNummer.Operator),
                            CrabMappings.ParseBewerking(terreinObjectHuisNummer.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObjectHuisNummer.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectHouseNumberFromCrab> GetCommandsFor(IEnumerable<tblTerreinObject_huisNummer> terreinObjectHuisNummers)
        {
            return terreinObjectHuisNummers
                .Select(
                    terreinObjectHuisNummer =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectHouseNumberFromCrab(
                            new CrabTerrainObjectHouseNumberId(terreinObjectHuisNummer.terreinObject_huisNummer_Id),
                            new CrabTerrainObjectId(terreinObjectHuisNummer.terreinObjectId),
                            new CrabHouseNumberId(terreinObjectHuisNummer.huisNummerId),
                            new CrabLifetime(terreinObjectHuisNummer.beginDatum.ToCrabLocalDateTime(), terreinObjectHuisNummer.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObjectHuisNummer.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObjectHuisNummer.Operator),
                            CrabMappings.ParseBewerking(terreinObjectHuisNummer.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObjectHuisNummer.Organisatie));
                    });
        }

        public static IEnumerable<ImportBuildingGeometryFromCrab> GetCommandsFor(IEnumerable<tblGebouwgeometrie_hist> gebouwGeometriesHist)
        {
            return gebouwGeometriesHist
                .Select(
                    gebouwgeometrie =>
                    {
                        MapLogging.Log(".");

                        return new ImportBuildingGeometryFromCrab(
                            new CrabBuildingGeometryId(gebouwgeometrie.gebouwgeometrieid.Value),
                            new CrabTerrainObjectId(gebouwgeometrie.terreinobjectid.Value),
                            new WkbGeometry(gebouwgeometrie.Geometry.WKB),
                            CrabMappings.ParseGebouwGeometrieMethode(gebouwgeometrie.MethodeGebouwgeometrie),
                            new CrabLifetime(gebouwgeometrie.begindatum?.ToCrabLocalDateTime(), gebouwgeometrie.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(gebouwgeometrie.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(gebouwgeometrie.Operator),
                            CrabMappings.ParseBewerking(gebouwgeometrie.Bewerking),
                            CrabMappings.ParseOrganisatie(gebouwgeometrie.Organisatie));
                    });
        }

        public static IEnumerable<ImportBuildingGeometryFromCrab> GetCommandsFor(IEnumerable<tblGebouwgeometrie> gebouwGeometries)
        {
            return gebouwGeometries
                .Select(
                    gebouwgeometrie =>
                    {
                        MapLogging.Log(".");

                        return new ImportBuildingGeometryFromCrab(
                            new CrabBuildingGeometryId(gebouwgeometrie.gebouwgeometrieid),
                            new CrabTerrainObjectId(gebouwgeometrie.terreinobjectid),
                            new WkbGeometry(gebouwgeometrie.Geometry.WKB),
                            CrabMappings.ParseGebouwGeometrieMethode(gebouwgeometrie.MethodeGebouwgeometrie),
                            new CrabLifetime(gebouwgeometrie.begindatum.ToCrabLocalDateTime(), gebouwgeometrie.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(gebouwgeometrie.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(gebouwgeometrie.Operator),
                            CrabMappings.ParseBewerking(gebouwgeometrie.Bewerking),
                            CrabMappings.ParseOrganisatie(gebouwgeometrie.Organisatie));
                    });
        }

        public static IEnumerable<ImportBuildingStatusFromCrab> GetCommandsFor(IEnumerable<tblGebouwstatus_hist> gebouwstatusesHist)
        {
            return gebouwstatusesHist
                .Select(
                    gebouwstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportBuildingStatusFromCrab(
                            new CrabBuildingStatusId(gebouwstatus.gebouwstatusid.Value),
                            new CrabTerrainObjectId(gebouwstatus.terreinobjectid.Value),
                            CrabMappings.ParseGebouwStatus(gebouwstatus.GebouwStatus),
                            new CrabLifetime(gebouwstatus.begindatum?.ToCrabLocalDateTime(), gebouwstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(gebouwstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(gebouwstatus.Operator),
                            CrabMappings.ParseBewerking(gebouwstatus.Bewerking),
                            CrabMappings.ParseOrganisatie(gebouwstatus.Organisatie));
                    });
        }

        public static IEnumerable<ImportBuildingStatusFromCrab> GetCommandsFor(IEnumerable<tblGebouwstatus> gebouwstatuses)
        {
            return gebouwstatuses
                .Select(
                    gebouwstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportBuildingStatusFromCrab(
                            new CrabBuildingStatusId(gebouwstatus.gebouwstatusid),
                            new CrabTerrainObjectId(gebouwstatus.terreinobjectid),
                            CrabMappings.ParseGebouwStatus(gebouwstatus.GebouwStatus),
                            new CrabLifetime(gebouwstatus.begindatum.ToCrabLocalDateTime(), gebouwstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(gebouwstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(gebouwstatus.Operator),
                            CrabMappings.ParseBewerking(gebouwstatus.Bewerking),
                            CrabMappings.ParseOrganisatie(gebouwstatus.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectFromCrab> GetCommandsFor(IEnumerable<tblTerreinObject_hist> terreinObjectsHist)
        {
            return terreinObjectsHist
                .Select(
                    terreinObject =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectFromCrab(
                            new CrabTerrainObjectId(terreinObject.terreinObjectId.Value),
                            new CrabIdentifierTerrainObject(terreinObject.identificatorTerreinObject),
                            new CrabTerrainObjectNatureCode(terreinObject.aardTerreinObjectCode),
                            terreinObject.x_coordinaat.HasValue ? new CrabCoordinate(terreinObject.x_coordinaat.Value) : null,
                            terreinObject.y_coordinaat.HasValue ? new CrabCoordinate(terreinObject.y_coordinaat.Value) : null,
                            new CrabBuildingNature(terreinObject.aardGebouw),
                            new CrabLifetime(terreinObject.beginDatum?.ToCrabLocalDateTime(), terreinObject.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObject.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObject.Operator),
                            CrabMappings.ParseBewerking(terreinObject.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObject.Organisatie));
                    });
        }

        public static ImportTerrainObjectFromCrab GetCommandsFor(tblTerreinObject terreinObject)
        {
            MapLogging.Log(".");
            return new ImportTerrainObjectFromCrab(
                new CrabTerrainObjectId(terreinObject.terreinObjectId),
                new CrabIdentifierTerrainObject(terreinObject.identificatorTerreinObject),
                new CrabTerrainObjectNatureCode(terreinObject.aardTerreinObjectCode),
                terreinObject.x_coordinaat.HasValue ? new CrabCoordinate(terreinObject.x_coordinaat.Value) : null,
                terreinObject.y_coordinaat.HasValue ? new CrabCoordinate(terreinObject.y_coordinaat.Value) : null,
                new CrabBuildingNature(terreinObject.aardGebouw),
                new CrabLifetime(terreinObject.beginDatum.ToCrabLocalDateTime(), terreinObject.eindDatum?.ToCrabLocalDateTime()),
                new CrabTimestamp(terreinObject.CrabTimestamp.ToCrabInstant()),
                new CrabOperator(terreinObject.Operator),
                CrabMappings.ParseBewerking(terreinObject.Bewerking),
                CrabMappings.ParseOrganisatie(terreinObject.Organisatie));
        }
    }
}
