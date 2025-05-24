using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class LogAuditMessageCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "MessageCategory",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageCategory",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
