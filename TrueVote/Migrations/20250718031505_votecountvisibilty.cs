using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueVote.Migrations
{
    /// <inheritdoc />
    public partial class votecountvisibilty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVoteCountVisible",
                table: "Polls",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVoteCountVisible",
                table: "Polls");
        }
    }
}
