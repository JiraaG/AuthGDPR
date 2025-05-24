using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class DataSubjectRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "DataSubjectRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseIdentity",
                table: "DataSubjectRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TraceIdentifier",
                table: "DataSubjectRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "DataSubjectRequests");

            migrationBuilder.DropColumn(
                name: "ResponseIdentity",
                table: "DataSubjectRequests");

            migrationBuilder.DropColumn(
                name: "TraceIdentifier",
                table: "DataSubjectRequests");
        }
    }
}
