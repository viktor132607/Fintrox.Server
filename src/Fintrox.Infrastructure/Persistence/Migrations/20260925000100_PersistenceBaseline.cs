using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FintroxDbContext))]
[Migration("20260925000100_PersistenceBaseline")]
public partial class PersistenceBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "core");
        migrationBuilder.EnsureSchema(name: "identity");
        migrationBuilder.EnsureSchema(name: "accounting");
        migrationBuilder.EnsureSchema(name: "sales");
        migrationBuilder.EnsureSchema(name: "purchases");
        migrationBuilder.EnsureSchema(name: "payments");
        migrationBuilder.EnsureSchema(name: "tax");
        migrationBuilder.EnsureSchema(name: "integration");
        migrationBuilder.EnsureSchema(name: "audit");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "audit";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "integration";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "tax";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "payments";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "purchases";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "sales";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "accounting";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "identity";""");
        migrationBuilder.Sql("""DROP SCHEMA IF EXISTS "core";""");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.12")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);
#pragma warning restore 612, 618
    }
}
