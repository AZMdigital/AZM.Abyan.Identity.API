using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class intialpermissonversion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeycloakPermissionId",
                table: "Permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeycloakResourceId",
                table: "Permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeycloakScopeId",
                table: "Permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Synced",
                table: "Permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeycloakPermissionId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "KeycloakResourceId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "KeycloakScopeId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Synced",
                table: "Permissions");
        }
    }
}
