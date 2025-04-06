using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.World.Migrations
{
    /// <inheritdoc />
    public partial class CreatureInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_template_property");

            migrationBuilder.DropTable(
                name: "entity_template_stat");

            migrationBuilder.DropTable(
                name: "entity_template");

            migrationBuilder.CreateTable(
                name: "creature_info_property",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    property = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    value = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.property });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "creature_info_stat",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    stat = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    value = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.stat });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "creature_info_property");

            migrationBuilder.DropTable(
                name: "creature_info_stat");

            migrationBuilder.CreateTable(
                name: "entity_template",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    displayInfo = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    faction1 = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false),
                    faction2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    outfitInfo = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false),
                    type = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_template", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "entity_template_property",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    property = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    value = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.property });
                    table.ForeignKey(
                        name: "FK__entity_template_property_id__entity_template_id",
                        column: x => x.id,
                        principalTable: "entity_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "entity_template_stat",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    stat = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    value = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.stat });
                    table.ForeignKey(
                        name: "FK__entity_template_stat_id__entity_template_id",
                        column: x => x.id,
                        principalTable: "entity_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
