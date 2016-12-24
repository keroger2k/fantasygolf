using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FantasyGolf.Core.Migrations
{
    public partial class AddingTournamentResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentResults",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    TournamentId = table.Column<int>(nullable: false),
                    Money = table.Column<decimal>(nullable: false),
                    Position = table.Column<string>(nullable: true),
                    R1 = table.Column<int>(nullable: false),
                    R2 = table.Column<int>(nullable: false),
                    R3 = table.Column<int>(nullable: false),
                    R4 = table.Column<int>(nullable: false),
                    Total = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentResults", x => new { x.PlayerId, x.Year, x.TournamentId });
                    table.ForeignKey(
                        name: "FK_TournamentResults_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentResults_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentResults_PlayerId",
                table: "TournamentResults",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentResults_TournamentId",
                table: "TournamentResults",
                column: "TournamentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentResults");
        }
    }
}
