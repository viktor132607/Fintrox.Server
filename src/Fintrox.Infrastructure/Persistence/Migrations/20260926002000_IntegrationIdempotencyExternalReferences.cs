using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationIdempotencyExternalReferences : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] IntegrationClientOrganizationColumns =
            ["integration_client_id", "organization_id"];

        private static readonly string[] OrganizationCreatedColumns =
            ["organization_id", "created_at_utc"];

        private static readonly string[] ExternalEventColumns =
            ["organization_id", "source_system", "external_id", "event_type"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integration_requests",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    request_method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    request_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    response_status_code = table.Column<int>(type: "integer", nullable: true),
                    response_content_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    response_body = table.Column<string>(type: "text", nullable: true),
                    resource_reference = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_integration_requests_integration_clients_integration_client~",
                        columns: x => new { x.integration_client_id, x.organization_id },
                        principalSchema: "integration",
                        principalTable: "integration_clients",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_integration_requests_integration_client_id_organization_id",
                schema: "integration",
                table: "integration_requests",
                columns: IntegrationClientOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_integration_requests_organization_created",
                schema: "integration",
                table: "integration_requests",
                columns: OrganizationCreatedColumns);

            migrationBuilder.CreateIndex(
                name: "ux_integration_requests_external_event",
                schema: "integration",
                table: "integration_requests",
                columns: ExternalEventColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_requests",
                schema: "integration");
        }
    }
}
