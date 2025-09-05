using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryExit.Migrations
{
    /// <inheritdoc />
    public partial class EntitiesCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GateEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    GateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateEntries_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GateExits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    GateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateExits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateExits_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GateEntries_GateId",
                table: "GateEntries",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_GateExits_GateId",
                table: "GateExits",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_Name",
                table: "Gates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_GateId",
                table: "Sensors",
                column: "GateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GateEntries");

            migrationBuilder.DropTable(
                name: "GateExits");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Gates");
        }
    }
}
