using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOfTheDimsTo256 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "Posts",
                type: "vector(256)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(512)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "Posts",
                type: "vector(512)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(256)",
                oldNullable: true);
        }
    }
}
