using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;
using CSharpFunctionalExtensions;

namespace Store.Consulting.Application.Services;

public sealed class DefaultCompatibilityRuleProvider : ICompatibilityRuleProvider
{
    private static readonly IReadOnlyCollection<CompatibilityRule> Rules =
    [
        CompatibilityRule.Create(
            "cpu_motherboard_socket",
            "CPU and motherboard socket",
            "Processors",
            "Motherboards",
            new CompatibilityCondition("socket", "socket", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{sourceProduct} socket {sourceValue} does not match {targetProduct} socket {targetValue}.",
            RecommendationType.Alternative).Value,

        CompatibilityRule.Create(
            "motherboard_ram_memory_type",
            "Motherboard and RAM memory type",
            "Motherboards",
            "RAM",
            new CompatibilityCondition("memoryType", "memoryType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{sourceProduct} requires {sourceValue} memory, but {targetProduct} is {targetValue}.",
            RecommendationType.Alternative).Value,

        CompatibilityRule.Create(
            "psu_gpu_power",
            "PSU capacity and GPU recommendation",
            "PowerSupplies",
            "GraphicsCards",
            new CompatibilityCondition("powerConsumptionWatts", "recommendedPsuWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Error,
            "{sourceProduct} capacity {sourceValue}W is below {targetProduct} recommended PSU {targetValue}W.",
            RecommendationType.RequiredPart).Value,

        CompatibilityRule.Create(
            "smartphone_charger_connector",
            "Smartphone and charger connector",
            "Smartphones",
            "Chargers",
            new CompatibilityCondition("connectorType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.").Value,

        CompatibilityRule.Create(
            "smartphone_charger_power",
            "Smartphone charger power",
            "Chargers",
            "Smartphones",
            new CompatibilityCondition("powerWatts", "requiredChargerWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Warning,
            "{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "laptop_charger_connector",
            "Laptop and charger connector",
            "Laptops",
            "Chargers",
            new CompatibilityCondition("connectorType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.").Value,

        CompatibilityRule.Create(
            "laptop_charger_power",
            "Laptop charger power",
            "Chargers",
            "Laptops",
            new CompatibilityCondition("powerWatts", "requiredChargerWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Warning,
            "{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "tablet_charger_connector",
            "Tablet and charger connector",
            "Tablets",
            "Chargers",
            new CompatibilityCondition("connectorType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.").Value,

        CompatibilityRule.Create(
            "tablet_charger_power",
            "Tablet charger power",
            "Chargers",
            "Tablets",
            new CompatibilityCondition("powerWatts", "requiredChargerWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Warning,
            "{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "smartphone_accessory_model",
            "Smartphone accessory device model",
            "Smartphones",
            "Accessories",
            new CompatibilityCondition("deviceModel", "compatibleDeviceModel", CompatibilityOperator.In),
            CompatibilitySeverity.Error,
            "{targetProduct} is intended for {targetValue}, not for {sourceProduct} ({sourceValue}).",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "laptop_accessory_model",
            "Laptop accessory device model",
            "Laptops",
            "Accessories",
            new CompatibilityCondition("deviceModel", "compatibleDeviceModel", CompatibilityOperator.In),
            CompatibilitySeverity.Warning,
            "{targetProduct} compatibility list {targetValue} does not include {sourceProduct} ({sourceValue}).",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "laptop_monitor_direct_connector",
            "Laptop and monitor direct connector",
            "Laptops",
            "Monitors",
            new CompatibilityCondition("connectorType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Warning,
            "{sourceProduct} connector {sourceValue} may need an adapter for {targetProduct} connector {targetValue}.",
            RecommendationType.RequiredPart).Value,

        CompatibilityRule.Create(
            "laptop_cable_input_connector",
            "Laptop and cable input connector",
            "Laptops",
            "CablesAdapters",
            new CompatibilityCondition("connectorType", "cableInputType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{targetProduct} input {targetValue} does not match {sourceProduct} connector {sourceValue}.",
            RecommendationType.Accessory).Value,

        CompatibilityRule.Create(
            "cable_monitor_output_connector",
            "Cable output and monitor connector",
            "CablesAdapters",
            "Monitors",
            new CompatibilityCondition("cableOutputType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{sourceProduct} output {sourceValue} does not match {targetProduct} connector {targetValue}.",
            RecommendationType.Accessory).Value
    ];

    public Task<Result<IReadOnlyCollection<CompatibilityRule>>> GetActiveRulesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(Rules));
}
