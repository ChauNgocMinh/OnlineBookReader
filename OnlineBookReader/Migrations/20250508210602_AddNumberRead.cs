using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBookReader.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberRead",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberRead",
                table: "Books");
        }
    }
}
