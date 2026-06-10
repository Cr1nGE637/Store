using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace Store.Catalog.Domain.ValueObjects;

public class ProductSpecification : ValueObject
{
    private static readonly IReadOnlySet<string> PositiveNumberKeys =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "powerConsumptionWatts",
            "recommendedPsuWatts",
            "screenSize",
            "refreshRate",
            "batteryCapacityMah",
            "batteryLifeHours",
            "powerWatts",
            "requiredChargerWatts"
        };

    private static readonly IReadOnlySet<string> NamedResolutions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HD",
            "Full HD",
            "FHD",
            "QHD",
            "UHD",
            "4K",
            "8K"
        };

    private static readonly IReadOnlyDictionary<string, string> CanonicalNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["socket"] = "socket",
            ["memorytype"] = "memoryType",
            ["formfactor"] = "formFactor",
            ["chipset"] = "chipset",
            ["interface"] = "interface",
            ["capacity"] = "capacity",
            ["powerconsumptionwatts"] = "powerConsumptionWatts",
            ["recommendedpsuwatts"] = "recommendedPsuWatts",
            ["connectortype"] = "connectorType",
            ["devicemodel"] = "deviceModel",
            ["compatibledevicemodel"] = "compatibleDeviceModel",
            ["compatibilitygroup"] = "compatibilityGroup",
            ["devicetype"] = "deviceType",
            ["memory"] = "memory",
            ["storage"] = "storage",
            ["processor"] = "processor",
            ["gpu"] = "gpu",
            ["screensize"] = "screenSize",
            ["diagonal"] = "screenSize",
            ["resolution"] = "resolution",
            ["refreshrate"] = "refreshRate",
            ["smarttv"] = "smartTv",
            ["batterycapacitymah"] = "batteryCapacityMah",
            ["batterylifehours"] = "batteryLifeHours",
            ["operatingsystem"] = "operatingSystem",
            ["connectiontype"] = "connectionType",
            ["powerwatts"] = "powerWatts",
            ["requiredchargerwatts"] = "requiredChargerWatts",
            ["fastchargingstandard"] = "fastChargingStandard",
            ["cableinputtype"] = "cableInputType",
            ["cableoutputtype"] = "cableOutputType",
            ["networkstandard"] = "networkStandard",
            ["frequencyband"] = "frequencyBand",
            ["ports"] = "ports",
            ["color"] = "color"
        };

    public string Name { get; }
    public string Value { get; }

    private ProductSpecification(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public static Result<ProductSpecification> Create(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductSpecification>("Specification name is required");

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<ProductSpecification>("Specification value is required");

        var normalizedName = NormalizeName(name);
        var normalizedValue = value.Trim();

        if (normalizedName.Length > 100)
            return Result.Failure<ProductSpecification>("Specification name cannot exceed 100 characters");

        if (normalizedValue.Length > 250)
            return Result.Failure<ProductSpecification>("Specification value cannot exceed 250 characters");

        var valueValidation = ValidateValue(normalizedName, normalizedValue);
        if (valueValidation.IsFailure)
            return Result.Failure<ProductSpecification>(valueValidation.Error);

        return Result.Success(new ProductSpecification(normalizedName, normalizedValue));
    }

    public static string NormalizeName(string name)
    {
        var trimmedName = name.Trim();
        var lookupKey = new string(trimmedName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

        return CanonicalNames.GetValueOrDefault(lookupKey, trimmedName);
    }

    private static Result ValidateValue(string normalizedName, string normalizedValue)
    {
        if (PositiveNumberKeys.Contains(normalizedName) && !IsPositiveNumberWithOptionalUnit(normalizedValue))
            return Result.Failure(
                $"Specification {normalizedName} must be a positive number, optionally followed by a unit");

        if (string.Equals(normalizedName, "resolution", StringComparison.OrdinalIgnoreCase)
            && !IsValidResolution(normalizedValue))
            return Result.Failure(
                "Specification resolution must be a common resolution name or a value like 1920x1080");

        return Result.Success();
    }

    private static bool IsPositiveNumberWithOptionalUnit(string value)
    {
        if (!Regex.IsMatch(
                value,
                @"^\d+(?:[.,]\d+)?\s*(?:w|watt|watts|вт|mah|мач|hz|гц|h|hour|hours|ч|in|inch|inches|""|дюйм|дюйма|дюймов)?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return false;

        var numericPart = Regex.Match(value, @"^\d+(?:[.,]\d+)?").Value.Replace(',', '.');
        return decimal.TryParse(numericPart, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var number)
            && number > 0;
    }

    private static bool IsValidResolution(string value) =>
        NamedResolutions.Contains(value)
        || Regex.IsMatch(
            value,
            @"^\d{3,5}\s*[xх×]\s*\d{3,5}$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static ProductSpecification Reconstitute(string name, string value)
    {
        var result = Create(name, value);
        if (result.IsFailure)
            throw new InvalidOperationException($"Corrupt product specification in storage: {result.Error}");

        return result.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name.ToUpperInvariant();
        yield return Value;
    }
}
