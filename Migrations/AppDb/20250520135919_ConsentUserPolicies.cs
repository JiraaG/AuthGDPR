using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ConsentUserPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentVersion",
                table: "UserConsents");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsentPolicyId",
                table: "UserConsents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserConsents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "UserConsents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserConsents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsentPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousConsentPolicyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentPolicies_ConsentPolicies_PreviousConsentPolicyId",
                        column: x => x.PreviousConsentPolicyId,
                        principalTable: "ConsentPolicies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserConsentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserConsentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsentHistories_UserConsents_UserConsentId",
                        column: x => x.UserConsentId,
                        principalTable: "UserConsents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_ConsentPolicyId",
                table: "UserConsents",
                column: "ConsentPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPolicies_PreviousConsentPolicyId",
                table: "ConsentPolicies",
                column: "PreviousConsentPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsentHistories_UserConsentId",
                table: "UserConsentHistories",
                column: "UserConsentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_ConsentPolicies_ConsentPolicyId",
                table: "UserConsents",
                column: "ConsentPolicyId",
                principalTable: "ConsentPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_ConsentPolicies_ConsentPolicyId",
                table: "UserConsents");

            migrationBuilder.DropTable(
                name: "ConsentPolicies");

            migrationBuilder.DropTable(
                name: "UserConsentHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserConsents_ConsentPolicyId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "ConsentPolicyId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserConsents");

            migrationBuilder.AddColumn<string>(
                name: "ConsentVersion",
                table: "UserConsents",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
