using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Consulting.Infrastructure.DbContexts;

#nullable disable

namespace Store.Consulting.Infrastructure.Migrations
{
    [DbContext(typeof(ConsultingDbContext))]
    [Migration("20260608100000_SeedConsumerElectronicsCompatibilityRules")]
    public partial class SeedConsumerElectronicsCompatibilityRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                        '40000000-0000-0000-0000-000000000004',
                        'smartphone_charger_connector',
                        'Smartphone and charger connector',
                        'Smartphones',
                        'Chargers',
                        'connectorType',
                        'connectorType',
                        'Equals',
                        NULL,
                        'Error',
                        '{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.',
                        NULL,
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000005',
                        'smartphone_charger_power',
                        'Smartphone charger power',
                        'Chargers',
                        'Smartphones',
                        'powerWatts',
                        'requiredChargerWatts',
                        'GreaterThanOrEqual',
                        NULL,
                        'Warning',
                        '{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000006',
                        'laptop_charger_connector',
                        'Laptop and charger connector',
                        'Laptops',
                        'Chargers',
                        'connectorType',
                        'connectorType',
                        'Equals',
                        NULL,
                        'Error',
                        '{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.',
                        NULL,
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000007',
                        'laptop_charger_power',
                        'Laptop charger power',
                        'Chargers',
                        'Laptops',
                        'powerWatts',
                        'requiredChargerWatts',
                        'GreaterThanOrEqual',
                        NULL,
                        'Warning',
                        '{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000008',
                        'tablet_charger_connector',
                        'Tablet and charger connector',
                        'Tablets',
                        'Chargers',
                        'connectorType',
                        'connectorType',
                        'Equals',
                        NULL,
                        'Error',
                        '{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.',
                        NULL,
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000009',
                        'tablet_charger_power',
                        'Tablet charger power',
                        'Chargers',
                        'Tablets',
                        'powerWatts',
                        'requiredChargerWatts',
                        'GreaterThanOrEqual',
                        NULL,
                        'Warning',
                        '{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000010',
                        'smartphone_accessory_model',
                        'Smartphone accessory device model',
                        'Smartphones',
                        'Accessories',
                        'deviceModel',
                        'compatibleDeviceModel',
                        'In',
                        NULL,
                        'Error',
                        '{targetProduct} is intended for {targetValue}, not for {sourceProduct} ({sourceValue}).',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000011',
                        'laptop_accessory_model',
                        'Laptop accessory device model',
                        'Laptops',
                        'Accessories',
                        'deviceModel',
                        'compatibleDeviceModel',
                        'In',
                        NULL,
                        'Warning',
                        '{targetProduct} compatibility list {targetValue} does not include {sourceProduct} ({sourceValue}).',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000012',
                        'laptop_monitor_direct_connector',
                        'Laptop and monitor direct connector',
                        'Laptops',
                        'Monitors',
                        'connectorType',
                        'connectorType',
                        'Equals',
                        NULL,
                        'Warning',
                        '{sourceProduct} connector {sourceValue} may need an adapter for {targetProduct} connector {targetValue}.',
                        'RequiredPart',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000013',
                        'laptop_cable_input_connector',
                        'Laptop and cable input connector',
                        'Laptops',
                        'CablesAdapters',
                        'connectorType',
                        'cableInputType',
                        'Equals',
                        NULL,
                        'Error',
                        '{targetProduct} input {targetValue} does not match {sourceProduct} connector {sourceValue}.',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    ),
                    (
                        '40000000-0000-0000-0000-000000000014',
                        'cable_monitor_output_connector',
                        'Cable output and monitor connector',
                        'CablesAdapters',
                        'Monitors',
                        'cableOutputType',
                        'connectorType',
                        'Equals',
                        NULL,
                        'Error',
                        '{sourceProduct} output {sourceValue} does not match {targetProduct} connector {targetValue}.',
                        'Accessory',
                        TRUE,
                        TIMESTAMPTZ '2026-06-08 00:00:00+00'
                    )
                ON CONFLICT ("Code") DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM consulting."CompatibilityRules"
                WHERE "Code" IN (
                    'smartphone_charger_connector',
                    'smartphone_charger_power',
                    'laptop_charger_connector',
                    'laptop_charger_power',
                    'tablet_charger_connector',
                    'tablet_charger_power',
                    'smartphone_accessory_model',
                    'laptop_accessory_model',
                    'laptop_monitor_direct_connector',
                    'laptop_cable_input_connector',
                    'cable_monitor_output_connector'
                );
                """);
        }
    }
}
