using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthGDPR.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ConsentNewAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousUserConsentId",
                table: "UserConsents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentType",
                table: "UserConsentHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ConsentType",
                table: "ConsentPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "ConsentPolicies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_PreviousUserConsentId",
                table: "UserConsents",
                column: "PreviousUserConsentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_UserConsents_PreviousUserConsentId",
                table: "UserConsents",
                column: "PreviousUserConsentId",
                principalTable: "UserConsents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_UserConsents_PreviousUserConsentId",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_UserConsents_PreviousUserConsentId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "PreviousUserConsentId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "ConsentType",
                table: "UserConsentHistories");

            migrationBuilder.DropColumn(
                name: "ConsentType",
                table: "ConsentPolicies");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "ConsentPolicies");
        }
    }
}
