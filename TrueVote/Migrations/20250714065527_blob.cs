using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueVote.Migrations
{
    /// <inheritdoc />
    public partial class blob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobUrl",
                table: "PollFiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobUrl",
                table: "PollFiles");
        }
    }
}
