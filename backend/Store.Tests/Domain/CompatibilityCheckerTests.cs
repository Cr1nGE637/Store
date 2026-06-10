using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;
using Store.Consulting.Domain.Services;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Tests.Domain;

public class CompatibilityCheckerTests
{
    [Fact]
    public void Check_WhenCpuAndMotherboardSocketMatch_ReturnsOk()
    {
        var checker = new CompatibilityChecker();
        var rule = SocketRule();

        var result = checker.Check(
            [
                Product("AMD Ryzen 5 7600", "Processors", new Dictionary<string, string> { ["socket"] = "AM5" }),
                Product("ASUS B650-Plus", "Motherboards", new Dictionary<string, string> { ["socket"] = "AM5" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Ok, result.Status);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Check_WhenCpuAndMotherboardSocketMismatch_ReturnsExplainableIssueAndRecommendation()
    {
        var checker = new CompatibilityChecker();
        var rule = SocketRule();

        var result = checker.Check(
            [
                Product("Intel Core i5-13400F", "Processors", new Dictionary<string, string> { ["socket"] = "LGA1700" }),
                Product("ASUS B650-Plus", "Motherboards", new Dictionary<string, string> { ["socket"] = "AM5" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Error, result.Status);
        var issue = Assert.Single(result.Issues);
        Assert.Equal("cpu_motherboard_socket", issue.RuleCode);
        Assert.Contains("Intel Core i5-13400F", issue.Message);
        Assert.Contains("LGA1700", issue.Message);
        Assert.Contains("AM5", issue.Message);
        Assert.Single(result.Recommendations);
    }

    [Fact]
    public void Check_WhenSpecificationIsMissing_ReturnsUnknownWithReason()
    {
        var checker = new CompatibilityChecker();

        var result = checker.Check(
            [
                Product("AMD Ryzen 5 7600", "Processors", new Dictionary<string, string> { ["socket"] = "AM5" }),
                Product("ASUS B650-Plus", "Motherboards", new Dictionary<string, string>())
            ],
            [SocketRule()]);

        Assert.Equal(ConsultationStatus.Unknown, result.Status);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(CompatibilitySeverity.Unknown, issue.Severity);
        Assert.Contains("unknown", issue.Message);
    }

    [Fact]
    public void Check_WhenPsuCapacityMeetsGpuRequirement_ReturnsOk()
    {
        var checker = new CompatibilityChecker();
        var rule = CompatibilityRule.Create(
            "gpu_psu_power",
            "GPU recommended PSU wattage",
            "PowerSupplies",
            "GraphicsCards",
            new CompatibilityCondition("powerConsumptionWatts", "recommendedPsuWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Error,
            "{sourceProduct} must be at least {targetValue}W for {targetProduct}.").Value;

        var result = checker.Check(
            [
                Product("Corsair RM750e", "PowerSupplies", new Dictionary<string, string> { ["powerConsumptionWatts"] = "750" }),
                Product("RTX 4070 SUPER", "GraphicsCards", new Dictionary<string, string> { ["recommendedPsuWatts"] = "650" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Ok, result.Status);
    }

    [Fact]
    public void Check_WhenChargerPowerIsBelowDeviceRequirement_ReturnsWarning()
    {
        var checker = new CompatibilityChecker();
        var rule = CompatibilityRule.Create(
            "smartphone_charger_power",
            "Smartphone charger power",
            "Chargers",
            "Smartphones",
            new CompatibilityCondition("powerWatts", "requiredChargerWatts", CompatibilityOperator.GreaterThanOrEqual),
            CompatibilitySeverity.Warning,
            "{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.",
            RecommendationType.Accessory).Value;

        var result = checker.Check(
            [
                Product("Basic USB-C Charger", "Chargers", new Dictionary<string, string> { ["powerWatts"] = "15" }),
                Product("Samsung Galaxy S24", "Smartphones", new Dictionary<string, string> { ["requiredChargerWatts"] = "25" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Warning, result.Status);
        var issue = Assert.Single(result.Issues);
        Assert.Equal("smartphone_charger_power", issue.RuleCode);
        Assert.Contains("15", issue.Message);
        Assert.Contains("25", issue.Message);
    }

    [Fact]
    public void Check_WhenAccessorySupportsDeviceModel_ReturnsOk()
    {
        var checker = new CompatibilityChecker();
        var rule = CompatibilityRule.Create(
            "smartphone_accessory_model",
            "Smartphone accessory device model",
            "Smartphones",
            "Accessories",
            new CompatibilityCondition("deviceModel", "compatibleDeviceModel", CompatibilityOperator.In),
            CompatibilitySeverity.Error,
            "{targetProduct} is intended for {targetValue}, not for {sourceProduct} ({sourceValue}).",
            RecommendationType.Accessory).Value;

        var result = checker.Check(
            [
                Product("Apple iPhone 15", "Smartphones", new Dictionary<string, string> { ["deviceModel"] = "iPhone 15" }),
                Product("Spigen Liquid Air Case", "Accessories", new Dictionary<string, string> { ["compatibleDeviceModel"] = "iPhone 15; Galaxy S24" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Ok, result.Status);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Check_WhenCableOutputMatchesMonitorConnector_ReturnsOk()
    {
        var checker = new CompatibilityChecker();
        var rule = CompatibilityRule.Create(
            "cable_monitor_output_connector",
            "Cable output and monitor connector",
            "CablesAdapters",
            "Monitors",
            new CompatibilityCondition("cableOutputType", "connectorType", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{sourceProduct} output {sourceValue} does not match {targetProduct} connector {targetValue}.",
            RecommendationType.Accessory).Value;

        var result = checker.Check(
            [
                Product("UGREEN USB-C to HDMI 4K Adapter", "CablesAdapters", new Dictionary<string, string> { ["cableOutputType"] = "HDMI" }),
                Product("LG UltraGear 27GP850-B", "Monitors", new Dictionary<string, string> { ["connectorType"] = "HDMI" })
            ],
            [rule]);

        Assert.Equal(ConsultationStatus.Ok, result.Status);
        Assert.Empty(result.Issues);
    }

    private static CompatibilityRule SocketRule() =>
        CompatibilityRule.Create(
            "cpu_motherboard_socket",
            "CPU and motherboard socket",
            "Processors",
            "Motherboards",
            new CompatibilityCondition("socket", "socket", CompatibilityOperator.Equals),
            CompatibilitySeverity.Error,
            "{sourceProduct} socket {sourceValue} does not match {targetProduct} socket {targetValue}.",
            RecommendationType.Alternative).Value;

    private static ConsultationProduct Product(
        string name,
        string categoryCode,
        IReadOnlyDictionary<string, string> specifications) =>
        new(Guid.NewGuid(), name, categoryCode, specifications);
}
