using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AZM.Abyan.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixClientTenantFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Tenants_tenantId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_tenantId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "tenantId",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Clients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_RealmId",
                table: "Clients",
                column: "RealmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Tenants_RealmId",
                table: "Clients",
                column: "RealmId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Tenants_RealmId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_RealmId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Clients");

            migrationBuilder.AddColumn<Guid>(
                name: "tenantId",
                table: "Clients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
        }
    }
}
