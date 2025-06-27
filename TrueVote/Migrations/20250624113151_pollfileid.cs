using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueVote.Migrations
{
    /// <inheritdoc />
    public partial class pollfileid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollFiles_Polls_PollId",
                table: "PollFiles");

            migrationBuilder.DropIndex(
                name: "IX_PollFiles_PollId",
                table: "PollFiles");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "PollFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "PoleFileId",
                table: "Polls",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Polls_PoleFileId",
                table: "Polls",
                column: "PoleFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_PollFiles_PoleFileId",
                table: "Polls",
                column: "PoleFileId",
                principalTable: "PollFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_PollFiles_PoleFileId",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_Polls_PoleFileId",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "PoleFileId",
                table: "Polls");

            migrationBuilder.AddColumn<Guid>(
                name: "PollId",
                table: "PollFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PollFiles_PollId",
                table: "PollFiles",
                column: "PollId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PollFiles_Polls_PollId",
                table: "PollFiles",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
