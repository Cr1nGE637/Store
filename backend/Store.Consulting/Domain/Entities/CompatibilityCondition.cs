using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Domain.Entities;

public sealed record CompatibilityCondition(
    string SourceSpecificationKey,
    string TargetSpecificationKey,
    CompatibilityOperator Operator,
    string? ExpectedValue = null)
{
    public string NormalizedSourceSpecificationKey => NormalizeSpecificationKey(SourceSpecificationKey);
    public string NormalizedTargetSpecificationKey => NormalizeSpecificationKey(TargetSpecificationKey);

    public static string NormalizeSpecificationKey(string key)
    {
        var lookupKey = new string(key
            .Trim()
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

        return lookupKey switch
        {
            "socket" => "socket",
            "memorytype" => "memoryType",
            "formfactor" => "formFactor",
            "chipset" => "chipset",
            "interface" => "interface",
            "capacity" => "capacity",
            "powerconsumptionwatts" => "powerConsumptionWatts",
            "recommendedpsuwatts" => "recommendedPsuWatts",
            "powerwatts" => "powerWatts",
            "requiredchargerwatts" => "requiredChargerWatts",
            "connectortype" => "connectorType",
            "devicemodel" => "deviceModel",
            "compatibledevicemodel" => "compatibleDeviceModel",
            "compatibilitygroup" => "compatibilityGroup",
            "cableinputtype" => "cableInputType",
            "cableoutputtype" => "cableOutputType",
            "connectiontype" => "connectionType",
            "networkstandard" => "networkStandard",
            "frequencyband" => "frequencyBand",
            "screensize" => "screenSize",
            "resolution" => "resolution",
            "refreshrate" => "refreshRate",
            "batterylifehours" => "batteryLifeHours",
            _ => key.Trim()
        };
    }
}
