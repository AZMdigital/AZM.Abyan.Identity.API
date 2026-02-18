using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeycloakPermissionId",
                table: "Permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakPermissionId",
                table: "Permissions",
                type: "uuid",
                nullable: true);
        }
    }
}
