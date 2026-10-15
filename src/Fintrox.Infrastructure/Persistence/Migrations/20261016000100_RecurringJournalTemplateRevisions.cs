using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fintrox.Infrastructure.Persistence.Migrations;

/// <summary>Append-only tenant-owned, single-successor recurring template version lineage.</summary>
[DbContext(typeof(FintroxDbContext))]
[Migration("20261016000100_RecurringJournalTemplateRevisions")]
public sealed class RecurringJournalTemplateRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE accounting.recurring_journal_template_revisions (
                id uuid PRIMARY KEY,
                organization_id uuid NOT NULL,
                root_template_id uuid NOT NULL,
                previous_template_id uuid NOT NULL,
                replacement_template_id uuid NOT NULL,
                version_number integer NOT NULL CHECK (version_number >= 2),
                effective_from_month date NOT NULL
                    CHECK (EXTRACT(DAY FROM effective_from_month) = 1),
                revised_by_user_id uuid NOT NULL,
                revised_at_utc timestamptz NOT NULL DEFAULT now(),
                CONSTRAINT uq_recurring_revision_previous
                    UNIQUE (organization_id,previous_template_id),
                CONSTRAINT uq_recurring_revision_replacement
                    UNIQUE (organization_id,replacement_template_id),
                CONSTRAINT uq_recurring_revision_version
                    UNIQUE (organization_id,root_template_id,version_number),
                CONSTRAINT ck_recurring_revision_distinct
                    CHECK (previous_template_id <> replacement_template_id),
                CONSTRAINT fk_recurring_revision_root
                    FOREIGN KEY (root_template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recurring_revision_previous
                    FOREIGN KEY (previous_template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_recurring_revision_replacement
                    FOREIGN KEY (replacement_template_id,organization_id)
                    REFERENCES accounting.recurring_journal_templates(id,organization_id)
                    ON DELETE RESTRICT
            );
            CREATE INDEX ix_recurring_revisions_root
                ON accounting.recurring_journal_template_revisions
                (organization_id,root_template_id,version_number);
            CREATE TRIGGER trg_recurring_revision_immutable
                BEFORE UPDATE OR DELETE
                ON accounting.recurring_journal_template_revisions
                FOR EACH ROW EXECUTE FUNCTION
                    accounting.guard_recurring_journal_evidence();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE accounting.recurring_journal_template_revisions;
            """);
    }
}
