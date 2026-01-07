using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removerealmtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_Realm_RealmId",
                table: "Client");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Policy_PolicyId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Resource_ResourceId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Scope_ScopeId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Policy_Roles_RoleId",
                table: "Policy");

            migrationBuilder.DropForeignKey(
                name: "FK_Resource_Scope_ScopeId",
                table: "Resource");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Client_ClientId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "Realm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scope",
                table: "Scope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Resource",
                table: "Resource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Policy",
                table: "Policy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Client",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Client_RealmId",
                table: "Client");

            migrationBuilder.RenameTable(
                name: "Scope",
                newName: "Scopes");

            migrationBuilder.RenameTable(
                name: "Resource",
                newName: "Resources");

            migrationBuilder.RenameTable(
                name: "Policy",
                newName: "Policies");

            migrationBuilder.RenameTable(
                name: "Client",
                newName: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_Resource_ScopeId",
                table: "Resources",
                newName: "IX_Resources_ScopeId");

            migrationBuilder.RenameIndex(
                name: "IX_Policy_RoleId",
                table: "Policies",
                newName: "IX_Policies_RoleId");

            migrationBuilder.AddColumn<Guid>(
                name: "tenantId",
                table: "Clients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scopes",
                table: "Scopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Resources",
                table: "Resources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Policies",
                table: "Policies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_tenantId",
                table: "Clients",
                column: "tenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Tenants_tenantId",
                table: "Clients",
                column: "tenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Roles_RoleId",
                table: "Policies",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Scopes_ScopeId",
                table: "Resources",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Clients_ClientId",
                table: "Roles",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Tenants_tenantId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Policies_PolicyId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Scopes_ScopeId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Roles_RoleId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Scopes_ScopeId",
                table: "Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Clients_ClientId",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scopes",
                table: "Scopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Resources",
                table: "Resources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Policies",
                table: "Policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_tenantId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "tenantId",
                table: "Clients");

            migrationBuilder.RenameTable(
                name: "Scopes",
                newName: "Scope");

            migrationBuilder.RenameTable(
                name: "Resources",
                newName: "Resource");

            migrationBuilder.RenameTable(
                name: "Policies",
                newName: "Policy");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "Client");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_ScopeId",
                table: "Resource",
                newName: "IX_Resource_ScopeId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_RoleId",
                table: "Policy",
                newName: "IX_Policy_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scope",
                table: "Scope",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Resource",
                table: "Resource",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Policy",
                table: "Policy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Client",
                table: "Client",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Realm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    KeycloakRealmId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Realm", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Client_RealmId",
                table: "Client",
                column: "RealmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Realm_RealmId",
                table: "Client",
                column: "RealmId",
                principalTable: "Realm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Policy_PolicyId",
                table: "Permissions",
                column: "PolicyId",
                principalTable: "Policy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Resource_ResourceId",
                table: "Permissions",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Scope_ScopeId",
                table: "Permissions",
                column: "ScopeId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Policy_Roles_RoleId",
                table: "Policy",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Resource_Scope_ScopeId",
                table: "Resource",
                column: "ScopeId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Client_ClientId",
                table: "Roles",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
