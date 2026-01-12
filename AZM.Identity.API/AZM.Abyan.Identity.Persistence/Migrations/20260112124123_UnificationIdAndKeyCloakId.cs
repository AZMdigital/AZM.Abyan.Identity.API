using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UnificationIdAndKeyCloakId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeycloakUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakRealmId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "KeycloakScopeId",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "KeycloakRoleId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "KeycloakResourceId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "KeycloakPolicyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "KeycloakPermissionId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "KeycloakClientId",
                table: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakRealmId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakScopeId",
                table: "Scopes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakRoleId",
                table: "Roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakResourceId",
                table: "Resources",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakPolicyId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakPermissionId",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakClientId",
                table: "Clients",
                type: "uuid",
                nullable: true);
        }
    }
}
