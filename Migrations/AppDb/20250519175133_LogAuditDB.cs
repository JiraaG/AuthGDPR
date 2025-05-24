using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class LogAuditDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");
        }
    }
}
