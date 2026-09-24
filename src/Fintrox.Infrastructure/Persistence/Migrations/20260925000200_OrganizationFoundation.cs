using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintrox.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FintroxDbContext))]
[Migration("20260925000200_OrganizationFoundation")]
public partial class OrganizationFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "organizations",
            schema: "core",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                base_currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                time_zone_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                registration_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                vat_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_organizations", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ux_organizations_slug",
            schema: "core",
            table: "organizations",
            column: "slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_organizations_country_registration_number",
            schema: "core",
            table: "organizations",
            columns: new[] { "country_code", "registration_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_organizations_country_vat_number",
            schema: "core",
            table: "organizations",
            columns: new[] { "country_code", "vat_number" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "organizations",
            schema: "core");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.12")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Fintrox.Domain.Organizations.Organization", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedNever()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("BaseCurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("character varying(3)")
                    .HasColumnName("base_currency_code");

                b.Property<string>("CountryCode")
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnType("character varying(2)")
                    .HasColumnName("country_code");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<bool>("IsActive")
                    .HasColumnType("boolean")
                    .HasColumnName("is_active");

                b.Property<string>("LegalName")
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)")
                    .HasColumnName("legal_name");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)")
                    .HasColumnName("name");

                b.Property<string>("RegistrationNumber")
                    .HasMaxLength(64)
                    .HasColumnType("character varying(64)")
                    .HasColumnName("registration_number");

                b.Property<string>("Slug")
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("character varying(80)")
                    .HasColumnName("slug");

                b.Property<string>("TimeZoneId")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("character varying(64)")
                    .HasColumnName("time_zone_id");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.Property<string>("VatNumber")
                    .HasMaxLength(64)
                    .HasColumnType("character varying(64)")
                    .HasColumnName("vat_number");

                b.HasKey("Id");

                b.HasIndex("Slug")
                    .IsUnique()
                    .HasDatabaseName("ux_organizations_slug");

                b.HasIndex("CountryCode", "RegistrationNumber")
                    .IsUnique()
                    .HasDatabaseName("ux_organizations_country_registration_number");

                b.HasIndex("CountryCode", "VatNumber")
                    .IsUnique()
                    .HasDatabaseName("ux_organizations_country_vat_number");

                b.ToTable("organizations", "core");
            });
#pragma warning restore 612, 618
    }
}
