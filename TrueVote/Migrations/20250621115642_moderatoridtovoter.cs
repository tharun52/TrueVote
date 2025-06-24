using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueVote.Migrations
{
    /// <inheritdoc />
    public partial class moderatoridtovoter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModeratorId",
                table: "Voters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Voters_ModeratorId",
                table: "Voters",
                column: "ModeratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Voters_Moderators_ModeratorId",
                table: "Voters",
                column: "ModeratorId",
                principalTable: "Moderators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voters_Moderators_ModeratorId",
                table: "Voters");

            migrationBuilder.DropIndex(
                name: "IX_Voters_ModeratorId",
                table: "Voters");

            migrationBuilder.DropColumn(
                name: "ModeratorId",
                table: "Voters");
        }
    }
}
