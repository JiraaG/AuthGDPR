using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class CustomErrorMiddleware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraceId",
                table: "AuditLogs");
        }
    }
}
