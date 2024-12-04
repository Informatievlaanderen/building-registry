using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Consumer.Address.Migrations
{
    /// <inheritdoc />
    public partial class AddOffsetOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OffsetOverrides",
                schema: "BuildingRegistryConsumerAddress",
                columns: table => new
                {
                    ConsumerGroupId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Offset = table.Column<long>(type: "bigint", nullable: false),
                    Configured = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffsetOverrides", x => x.ConsumerGroupId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OffsetOverrides",
                schema: "BuildingRegistryConsumerAddress");
        }
    }
}
