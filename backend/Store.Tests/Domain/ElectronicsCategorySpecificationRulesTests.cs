using Store.Catalog.Domain.Enums;
using Store.Catalog.Domain.Services;

namespace Store.Tests.Domain;

public class ElectronicsCategorySpecificationRulesTests
{
    [Fact]
    public void ValidateRequiredSpecifications_WhenMotherboardMemoryTypeIsMissing_ReturnsFailure()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Motherboards,
            new Dictionary<string, string>
            {
                ["socket"] = "AM5",
                ["formFactor"] = "ATX",
                ["chipset"] = "B650"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("memoryType", result.Error);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenSsdUsesFormFactorAlias_ReturnsSuccess()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.SSD,
            new Dictionary<string, string>
            {
                ["capacity"] = "2TB",
                ["interface"] = "PCIe 4.0 NVMe",
                ["form_factor"] = "M.2 2280"
            });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenGraphicsCardRecommendedPsuIsMissing_ReturnsFailure()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.GraphicsCards,
            new Dictionary<string, string>
            {
                ["chipset"] = "AD104",
                ["interface"] = "PCIe 4.0",
                ["powerConsumptionWatts"] = "220"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("recommendedPsuWatts", result.Error);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenProcessorUsesCamelCasePowerKey_ReturnsSuccess()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Processors,
            new Dictionary<string, string>
            {
                ["socket"] = "AM5",
                ["chipset"] = "Zen 4",
                ["powerConsumptionWatts"] = "65"
            });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenSmartphoneBatteryIsMissing_ReturnsFailure()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Smartphones,
            new Dictionary<string, string>
            {
                ["deviceModel"] = "Galaxy S24",
                ["storage"] = "256GB",
                ["screenSize"] = "6.2",
                ["connectorType"] = "USB-C",
                ["operatingSystem"] = "Android"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("batteryCapacityMah", result.Error);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenChargerUsesSnakeCasePowerKey_ReturnsSuccess()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Chargers,
            new Dictionary<string, string>
            {
                ["power_watts"] = "65",
                ["connector_type"] = "USB-C",
                ["fast_charging_standard"] = "USB PD"
            });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenTelevisionUsesDiagonalAlias_ReturnsSuccess()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Televisions,
            new Dictionary<string, string>
            {
                ["diagonal"] = "55",
                ["resolution"] = "4K",
                ["smartTv"] = "webOS",
                ["refreshRate"] = "120Hz",
                ["connectorType"] = "HDMI"
            });

        Assert.True(result.IsSuccess);
    }
}
