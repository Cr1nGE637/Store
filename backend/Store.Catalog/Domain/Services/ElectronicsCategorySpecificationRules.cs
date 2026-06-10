using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Enums;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Domain.Services;

public static class ElectronicsCategorySpecificationRules
{
    private static readonly IReadOnlyDictionary<ElectronicsCategoryCode, IReadOnlyCollection<RequiredSpecification>> RequiredKeys =
        new Dictionary<ElectronicsCategoryCode, IReadOnlyCollection<RequiredSpecification>>
        {
            [ElectronicsCategoryCode.Processors] =
            [
                new("socket", "socket"),
                new("chipset", "chipset"),
                new("powerconsumptionwatts", "powerConsumptionWatts")
            ],
            [ElectronicsCategoryCode.Motherboards] =
            [
                new("socket", "socket"),
                new("memorytype", "memoryType"),
                new("formfactor", "formFactor"),
                new("chipset", "chipset")
            ],
            [ElectronicsCategoryCode.RAM] =
            [
                new("memorytype", "memoryType"),
                new("capacity", "capacity"),
                new("formfactor", "formFactor")
            ],
            [ElectronicsCategoryCode.SSD] =
            [
                new("capacity", "capacity"),
                new("interface", "interface"),
                new("formfactor", "formFactor")
            ],
            [ElectronicsCategoryCode.GraphicsCards] =
            [
                new("chipset", "chipset"),
                new("interface", "interface"),
                new("powerconsumptionwatts", "powerConsumptionWatts"),
                new("recommendedpsuwatts", "recommendedPsuWatts")
            ],
            [ElectronicsCategoryCode.PowerSupplies] =
            [
                new("powerconsumptionwatts", "powerConsumptionWatts"),
                new("formfactor", "formFactor"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.Laptops] =
            [
                new("devicemodel", "deviceModel"),
                new("processor", "processor"),
                new("memory", "memory"),
                new("storage", "storage"),
                new("screensize", "screenSize"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.Smartphones] =
            [
                new("devicemodel", "deviceModel"),
                new("storage", "storage"),
                new("screensize", "screenSize"),
                new("batterycapacitymah", "batteryCapacityMah"),
                new("connectortype", "connectorType"),
                new("operatingsystem", "operatingSystem")
            ],
            [ElectronicsCategoryCode.Tablets] =
            [
                new("devicemodel", "deviceModel"),
                new("storage", "storage"),
                new("screensize", "screenSize"),
                new("batterycapacitymah", "batteryCapacityMah"),
                new("connectortype", "connectorType"),
                new("operatingsystem", "operatingSystem")
            ],
            [ElectronicsCategoryCode.Televisions] =
            [
                new("screensize", "screenSize"),
                new("resolution", "resolution"),
                new("smarttv", "smartTv"),
                new("refreshrate", "refreshRate"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.Headphones] =
            [
                new("connectiontype", "connectionType"),
                new("connectortype", "connectorType"),
                new("batterylifehours", "batteryLifeHours")
            ],
            [ElectronicsCategoryCode.Chargers] =
            [
                new("powerwatts", "powerWatts"),
                new("connectortype", "connectorType"),
                new("fastchargingstandard", "fastChargingStandard")
            ],
            [ElectronicsCategoryCode.CablesAdapters] =
            [
                new("cableinputtype", "cableInputType"),
                new("cableoutputtype", "cableOutputType"),
                new("interface", "interface")
            ],
            [ElectronicsCategoryCode.SmartWatches] =
            [
                new("devicemodel", "deviceModel"),
                new("screensize", "screenSize"),
                new("batterylifehours", "batteryLifeHours"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.GameConsoles] =
            [
                new("devicemodel", "deviceModel"),
                new("storage", "storage"),
                new("resolution", "resolution"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.NetworkEquipment] =
            [
                new("networkstandard", "networkStandard"),
                new("frequencyband", "frequencyBand"),
                new("interface", "interface")
            ],
            [ElectronicsCategoryCode.Accessories] =
            [
                new("compatibilitygroup", "compatibilityGroup"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.Peripherals] =
            [
                new("devicetype", "deviceType"),
                new("interface", "interface"),
                new("connectortype", "connectorType")
            ],
            [ElectronicsCategoryCode.Monitors] =
            [
                new("screensize", "screenSize"),
                new("resolution", "resolution"),
                new("refreshrate", "refreshRate"),
                new("interface", "interface")
            ]
        };

    public static Result ValidateRequiredSpecifications(
        ElectronicsCategoryCode categoryCode,
        IReadOnlyDictionary<string, string>? specifications)
    {
        var providedKeys = (specifications ?? new Dictionary<string, string>())
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .Select(s => ProductSpecification.NormalizeName(s.Key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingKeys = RequiredKeys[categoryCode]
            .Where(specification => !providedKeys.Contains(specification.NormalizedKey))
            .Select(specification => specification.DisplayName)
            .ToList();

        if (missingKeys.Count == 0)
            return Result.Success();

        return Result.Failure(
            $"Missing required specifications for {categoryCode}: {string.Join(", ", missingKeys)}");
    }

    private sealed record RequiredSpecification(string NormalizedKey, string DisplayName);
}
