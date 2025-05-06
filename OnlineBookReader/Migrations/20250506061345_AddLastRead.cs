using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBookReader.Migrations
{
    /// <inheritdoc />
    public partial class AddLastRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastReadId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastReadId",
                table: "AspNetUsers",
                column: "LastReadId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Books_LastReadId",
                table: "AspNetUsers",
                column: "LastReadId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Books_LastReadId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LastReadId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastReadId",
                table: "AspNetUsers");
        }
    }
}
