using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    using Infrastructure;

    public partial class AddViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Schema.Wms}].[GebouwView]
            WITH SCHEMABINDING
            AS
            SELECT        PersistentLocalId AS ObjectId, Id, Version, geometry::STGeomFromWKB([Geometry], 0) AS Geometry, GeometryMethod AS GeometrieMethode, Status
            FROM            {Schema.Wms}.{Building.BuildingConfiguration.TableName}
            WHERE        (IsComplete = 1)
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGepland]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gepland' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Planned'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGehistoreerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gehistoreerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Retired'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gerealiseerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Realized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwNietGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'NietGerealiseerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'NotRealized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwInAanbouw]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'InAanbouw' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'UnderConstruction'
            GO");

            migrationBuilder.Sql($@"
            CREATE VIEW [{Schema.Wms}].[GebouweenheidView]
            WITH SCHEMABINDING
            AS
            SELECT        Id, BuildingUnitPersistentLocalId AS ObjectId, Version AS VersieId, PositionMethod AS PositieGeometrieMethode, Status AS GebouweenheidStatus, [Function] AS Functie, BuildingPersistentLocalId AS GebouwObjectId, geometry::STGeomFromWKB([Position], 0) AS Geometrie
            FROM            {Schema.Wms}.{BuildingUnit.BuildingUnitConfiguration.TableName}
            WHERE        (IsComplete = 1) AND (IsBuildingComplete = 1)
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGehistoreerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gehistoreerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Retired'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGepland]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gepland' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Planned'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gerealiseerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Realized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidNietGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'NietGerealiseerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'NotRealized'
            GO
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            DROP VIEW [{Schema.Wms}].[GebouweenheidView]
            DROP VIEW [{Schema.Wms}].[GebouweenheidGehistoreerd]
            DROP VIEW [{Schema.Wms}].[GebouweenheidGepland]
            DROP VIEW [{Schema.Wms}].[GebouweenheidGerealiseerd]
            DROP VIEW [{Schema.Wms}].[GebouweenheidNietGerealiseerd]
            ");

            migrationBuilder.Sql($@"
            DROP VIEW [{Schema.Wms}].[GebouwGepland]
            DROP VIEW [{Schema.Wms}].[GebouwGehistoreerd]
            DROP VIEW [{Schema.Wms}].[GebouwGerealiseerd]
            DROP VIEW [{Schema.Wms}].[GebouwNietGerealiseerd]
            DROP VIEW [{Schema.Wms}].[GebouwInAanbouw]
            DROP VIEW [{Schema.Wms}].[GebouwView]
            GO");
        }
    }
}
