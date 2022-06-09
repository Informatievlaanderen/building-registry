using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Api.CrabImport.Migrations
{
    using BuildingRegistry.Infrastructure;

    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
              CREATE SEQUENCE {Schema.Sequence}.{SequenceContext.BuildingPersistentLocalIdSequenceName}
                AS int
                START WITH 30000000
                INCREMENT BY 1
	            MINVALUE 1
                MAXVALUE 999999999
                NO CYCLE
                NO CACHE
            ;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP SEQUENCE {Schema.Sequence}.{SequenceContext.BuildingPersistentLocalIdSequenceName};");
        }
    }
}
