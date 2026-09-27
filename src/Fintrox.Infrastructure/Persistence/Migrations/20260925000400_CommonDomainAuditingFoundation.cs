using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommonDomainAuditingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "core",
                table: "organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "core",
                table: "organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "identity",
                table: "organization_memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "identity",
                table: "organization_memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    changes_json = table.Column<string>(type: "jsonb", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_organization_occurred",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "organization_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_occurred",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "user_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "audit");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "core",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "core",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "identity",
                table: "organization_memberships");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "identity",
                table: "organization_memberships");
        }
    }
}
