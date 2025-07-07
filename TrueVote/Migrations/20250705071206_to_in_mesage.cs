using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueVote.Migrations
{
    /// <inheritdoc />
    public partial class to_in_mesage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "From",
                table: "Messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "Messages");
        }
    }
}
