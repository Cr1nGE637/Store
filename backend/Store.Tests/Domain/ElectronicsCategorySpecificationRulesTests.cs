using Store.Catalog.Domain.Enums;
using Store.Catalog.Domain.Services;

namespace Store.Tests.Domain;

public class ElectronicsCategorySpecificationRulesTests
{
    [Fact]
    public void ValidateRequiredSpecifications_WhenSmartphoneMemoryIsMissing_ReturnsFailure()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Smartphone,
            new Dictionary<string, string>
            {
                ["display"] = "6.1 inch",
                ["processor"] = "A16 Bionic"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("memory", result.Error);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenStorageUsesFormFactorAlias_ReturnsSuccess()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Storage,
            new Dictionary<string, string>
            {
                ["capacity"] = "2TB",
                ["interface"] = "PCIe 4.0 NVMe",
                ["form_factor"] = "M.2 2280"
            });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRequiredSpecifications_WhenMonitorRefreshRateIsMissing_ReturnsFailure()
    {
        var result = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            ElectronicsCategoryCode.Monitor,
            new Dictionary<string, string>
            {
                ["display"] = "27 inch",
                ["resolution"] = "2560x1440"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("refresh_rate", result.Error);
    }
}
