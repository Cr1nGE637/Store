using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Consulting.Infrastructure.DbContexts;

#nullable disable

namespace Store.Consulting.Infrastructure.Migrations
{
    [DbContext(typeof(ConsultingDbContext))]
    [Migration("20260606090000_InitialConsulting")]
    public partial class InitialConsulting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "consulting");

            migrationBuilder.CreateTable(
                name: "CompatibilityRules",
                schema: "consulting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SourceCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceSpecificationKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetSpecificationKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MessageTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecommendationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompatibilityRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationResults",
                schema: "consulting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ItemsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FindingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    RecommendationsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationEvents",
                schema: "consulting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompatibilityRules_Code",
                schema: "consulting",
                table: "CompatibilityRules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompatibilityRules_SourceCategoryCode_TargetCategoryCode_IsActive",
                schema: "consulting",
                table: "CompatibilityRules",
                columns: new[] { "SourceCategoryCode", "TargetCategoryCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationResults_CheckedAtUtc",
                schema: "consulting",
                table: "ConsultationResults",
                column: "CheckedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationResults_CustomerId",
                schema: "consulting",
                table: "ConsultationResults",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_ConsultationId",
                schema: "consulting",
                table: "RecommendationEvents",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_ProductId",
                schema: "consulting",
                table: "RecommendationEvents",
                column: "ProductId");

            SeedDefaultRules(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RecommendationEvents", schema: "consulting");
            migrationBuilder.DropTable(name: "ConsultationResults", schema: "consulting");
            migrationBuilder.DropTable(name: "CompatibilityRules", schema: "consulting");
        }

        private static void SeedDefaultRules(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO consulting."CompatibilityRules" (
                    "Id",
                    "Code",
                    "Name",
                    "SourceCategoryCode",
                    "TargetCategoryCode",
                    "SourceSpecificationKey",
                    "TargetSpecificationKey",
                    "Operator",
                    "ExpectedValue",
                    "Severity",
                    "MessageTemplate",
                    "RecommendationType",
                    "IsActive",
                    "CreatedAtUtc")
                VALUES
                    (
                        '40000000-0000-0000-0000-000000000001',
                        'cpu_motherboard_socket',
                        'CPU and motherboard socket',
                        'Processors',
                        'Motherboards',
                        'socket',
                        'socket',
                        'Equals',
                        NULL,
                        'Error',
                        '{sourceProduct} socket {sourceValue} does not match {targetProduct} socket {targetValue}.',
                        'Alternative',
                        TRUE,
                        TIMESTAMPTZ '2026-06-06 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000002',
                        'motherboard_ram_memory_type',
                        'Motherboard and RAM memory type',
                        'Motherboards',
                        'RAM',
                        'memoryType',
                        'memoryType',
                        'Equals',
                        NULL,
                        'Error',
                        '{sourceProduct} requires {sourceValue} memory, but {targetProduct} is {targetValue}.',
                        'Alternative',
                        TRUE,
                        TIMESTAMPTZ '2026-06-06 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000003',
                        'psu_gpu_power',
                        'PSU capacity and GPU recommendation',
                        'PowerSupplies',
                        'GraphicsCards',
                        'powerConsumptionWatts',
                        'recommendedPsuWatts',
                        'GreaterThanOrEqual',
                        NULL,
                        'Error',
                        '{sourceProduct} capacity {sourceValue}W is below {targetProduct} recommended PSU {targetValue}W.',
                        'RequiredPart',
                        TRUE,
                        TIMESTAMPTZ '2026-06-06 00:00:00+00'
                    )
                ON CONFLICT ("Code") DO NOTHING;
                """);
        }
    }
}
