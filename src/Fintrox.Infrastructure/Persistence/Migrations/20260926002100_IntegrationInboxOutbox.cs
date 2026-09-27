using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationInboxOutbox : Migration
    {
        private static readonly string[] IdOrganizationColumns =
            ["id", "organization_id"];

        private static readonly string[] InboxOrganizationColumns =
            ["inbox_id", "organization_id"];

        private static readonly string[] OrganizationTypeOccurredColumns =
            ["organization_id", "event_type", "occurred_at_utc"];

        private static readonly string[] OrganizationResolvedAttemptColumns =
            ["organization_id", "is_resolved", "last_attempt_at_utc"];

        private static readonly string[] OrganizationKindReferenceColumns =
            ["organization_id", "kind", "reference_id"];

        private static readonly string[] OrganizationStatusCreatedColumns =
            ["organization_id", "status", "created_at_utc"];

        private static readonly string[] ExternalEventColumns =
            ["organization_id", "source_system", "external_id", "event_type"];

        private static readonly string[] EventOrganizationColumns =
            ["integration_event_id", "organization_id"];

        private static readonly string[] StatusNextAttemptColumns =
            ["status", "next_attempt_at_utc"];

        private static readonly string[] SubscriptionCreatedColumns =
            ["organization_id", "webhook_subscription_id", "created_at_utc"];

        private static readonly string[] SubscriptionOrganizationColumns =
            ["webhook_subscription_id", "organization_id"];

        private static readonly string[] EventSubscriptionColumns =
            ["integration_event_id", "webhook_subscription_id"];

        private static readonly string[] OrganizationActiveNameColumns =
            ["organization_id", "is_active", "name"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integration_failures",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_failures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_inbox",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    result_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    result_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_inbox", x => x.id);
                    table.UniqueConstraint("ak_integration_inbox_id_organization", x => new { x.id, x.organization_id });
                });

            migrationBuilder.CreateTable(
                name: "webhook_subscriptions",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    target_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    event_types = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    signing_secret_ciphertext = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    secret_prefix = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    secret_rotated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_subscriptions", x => x.id);
                    table.UniqueConstraint("ak_webhook_subscriptions_id_organization", x => new { x.id, x.organization_id });
                });

            migrationBuilder.CreateTable(
                name: "integration_events",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inbox_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    dispatched_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_events", x => x.id);
                    table.UniqueConstraint("ak_integration_events_id_organization", x => new { x.id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_integration_events_integration_inbox_inbox_id_organization_~",
                        columns: x => new { x.inbox_id, x.organization_id },
                        principalSchema: "integration",
                        principalTable: "integration_inbox",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "webhook_deliveries",
                schema: "integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_status_code = table.Column<int>(type: "integer", nullable: true),
                    last_error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    delivered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "FK_webhook_deliveries_integration_events_integration_event_id_~",
                        columns: x => new { x.integration_event_id, x.organization_id },
                        principalSchema: "integration",
                        principalTable: "integration_events",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_webhook_deliveries_webhook_subscriptions_webhook_subscripti~",
                        columns: x => new { x.webhook_subscription_id, x.organization_id },
                        principalSchema: "integration",
                        principalTable: "webhook_subscriptions",
                        principalColumns: IdOrganizationColumns,
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_integration_events_inbox_id_organization_id",
                schema: "integration",
                table: "integration_events",
                columns: InboxOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_integration_events_organization_type_occurred",
                schema: "integration",
                table: "integration_events",
                columns: OrganizationTypeOccurredColumns);

            migrationBuilder.CreateIndex(
                name: "ix_integration_failures_organization_resolved_attempt",
                schema: "integration",
                table: "integration_failures",
                columns: OrganizationResolvedAttemptColumns);

            migrationBuilder.CreateIndex(
                name: "ux_integration_failures_open_reference",
                schema: "integration",
                table: "integration_failures",
                columns: OrganizationKindReferenceColumns,
                unique: true,
                filter: "is_resolved = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_integration_inbox_organization_status_created",
                schema: "integration",
                table: "integration_inbox",
                columns: OrganizationStatusCreatedColumns);

            migrationBuilder.CreateIndex(
                name: "ux_integration_inbox_external_event",
                schema: "integration",
                table: "integration_inbox",
                columns: ExternalEventColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_deliveries_integration_event_id_organization_id",
                schema: "integration",
                table: "webhook_deliveries",
                columns: EventOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_status_next_attempt",
                schema: "integration",
                table: "webhook_deliveries",
                columns: StatusNextAttemptColumns);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_subscription_created",
                schema: "integration",
                table: "webhook_deliveries",
                columns: SubscriptionCreatedColumns);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_deliveries_webhook_subscription_id_organization_id",
                schema: "integration",
                table: "webhook_deliveries",
                columns: SubscriptionOrganizationColumns);

            migrationBuilder.CreateIndex(
                name: "ux_webhook_deliveries_event_subscription",
                schema: "integration",
                table: "webhook_deliveries",
                columns: EventSubscriptionColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_subscriptions_organization_active_name",
                schema: "integration",
                table: "webhook_subscriptions",
                columns: OrganizationActiveNameColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_failures",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "webhook_deliveries",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_events",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "webhook_subscriptions",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_inbox",
                schema: "integration");
        }
    }
}
