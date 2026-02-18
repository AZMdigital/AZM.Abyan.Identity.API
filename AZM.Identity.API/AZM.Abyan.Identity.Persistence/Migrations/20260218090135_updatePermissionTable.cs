using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatePermissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "Permissions");

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakPermissionId",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PolicyId",
                table: "Permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceId",
                table: "Permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ScopeId",
                table: "Permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PolicyId",
                table: "Permissions",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ResourceId",
                table: "Permissions",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ScopeId",
                table: "Permissions",
                column: "ScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Policies_PolicyId",
                table: "Permissions",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                table: "Permissions",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Scopes_ScopeId",
                table: "Permissions",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Policies_PolicyId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Scopes_ScopeId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_PolicyId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ResourceId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ScopeId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "KeycloakPermissionId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "PolicyId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                table: "Permissions");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "Permissions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
