using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class AddRecordProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Record_All",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Record_All",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Record_All",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_Channel",
                table: "Record_All",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_Platform",
                table: "Record_All",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_Version",
                table: "Record_All",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Record_All_Channel",
                table: "Record_All");

            migrationBuilder.DropIndex(
                name: "IX_Record_All_Platform",
                table: "Record_All");

            migrationBuilder.DropIndex(
                name: "IX_Record_All_Version",
                table: "Record_All");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Record_All");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Record_All");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Record_All");
        }
    }
}
