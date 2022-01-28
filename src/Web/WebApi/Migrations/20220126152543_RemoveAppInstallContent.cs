using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class RemoveAppInstallContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Desktop_UpdateVersions_Time",
                table: "Desktop_UpdateVersions");

            migrationBuilder.DropColumn(
                name: "AppInstallerContent",
                table: "Desktop_UpdateVersions");

            migrationBuilder.CreateIndex(
                name: "IX_Desktop_UpdateVersions_Version",
                table: "Desktop_UpdateVersions",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Desktop_UpdateVersions_Version",
                table: "Desktop_UpdateVersions");

            migrationBuilder.AddColumn<string>(
                name: "AppInstallerContent",
                table: "Desktop_UpdateVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Desktop_UpdateVersions_Time",
                table: "Desktop_UpdateVersions",
                column: "Time");
        }
    }
}
