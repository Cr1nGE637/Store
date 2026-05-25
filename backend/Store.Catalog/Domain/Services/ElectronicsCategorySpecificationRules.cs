using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Enums;

namespace Store.Catalog.Domain.Services;

public static class ElectronicsCategorySpecificationRules
{
    private static readonly IReadOnlyDictionary<ElectronicsCategoryCode, IReadOnlyCollection<RequiredSpecification>> RequiredKeys =
        new Dictionary<ElectronicsCategoryCode, IReadOnlyCollection<RequiredSpecification>>
        {
            [ElectronicsCategoryCode.Smartphone] =
            [
                new("memory", "memory"),
                new("display", "display"),
                new("processor", "processor")
            ],
            [ElectronicsCategoryCode.Laptop] =
            [
                new("memory", "memory"),
                new("display", "display"),
                new("processor", "processor"),
                new("storage", "storage")
            ],
            [ElectronicsCategoryCode.Storage] =
            [
                new("capacity", "capacity"),
                new("interface", "interface"),
                new("formfactor", "form_factor")
            ],
            [ElectronicsCategoryCode.GraphicsCard] =
            [
                new("memory", "memory"),
                new("chipset", "chipset"),
                new("interface", "interface")
            ],
            [ElectronicsCategoryCode.Monitor] =
            [
                new("display", "display"),
                new("resolution", "resolution"),
                new("refreshrate", "refresh_rate")
            ],
            [ElectronicsCategoryCode.Peripheral] = [new("connectiontype", "connection_type")],
            [ElectronicsCategoryCode.Accessory] = [new("compatibility", "compatibility")]
        };

    public static Result ValidateRequiredSpecifications(
        ElectronicsCategoryCode categoryCode,
        IReadOnlyDictionary<string, string>? specifications)
    {
        var providedKeys = (specifications ?? new Dictionary<string, string>())
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .Select(s => NormalizeKey(s.Key))
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

    private static string NormalizeKey(string key) =>
        new(key
            .Trim()
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

    private sealed record RequiredSpecification(string NormalizedKey, string DisplayName);
}
