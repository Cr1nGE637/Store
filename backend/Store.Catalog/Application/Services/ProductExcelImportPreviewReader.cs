using System.Globalization;
using ClosedXML.Excel;
using CSharpFunctionalExtensions;

namespace Store.Catalog.Application.Services;

public sealed class ProductExcelImportPreviewReader
{
    private static readonly IReadOnlyCollection<string> RequiredProductHeaders =
    [
        "sku",
        "name",
        "description",
        "categorycode",
        "price",
        "stockquantity",
        "isactive"
    ];

    public Result<ProductExcelImportWorkbook> Read(Stream content)
    {
        try
        {
            using var workbook = new XLWorkbook(content);
            var productsSheet = workbook.Worksheets.FirstOrDefault(sheet =>
                string.Equals(sheet.Name, "Products", StringComparison.OrdinalIgnoreCase));
            if (productsSheet is null)
                return Result.Failure<ProductExcelImportWorkbook>("Products sheet is required");

            var productHeaders = ReadHeaders(productsSheet);
            var missingHeaders = RequiredProductHeaders
                .Where(header => !productHeaders.ContainsKey(header))
                .ToArray();
            if (missingHeaders.Length > 0)
            {
                return Result.Failure<ProductExcelImportWorkbook>(
                    $"Products sheet is missing columns: {string.Join(", ", missingHeaders)}");
            }

            var products = ReadProducts(productsSheet, productHeaders);
            var specificationsSheet = workbook.Worksheets.FirstOrDefault(sheet =>
                string.Equals(sheet.Name, "Specifications", StringComparison.OrdinalIgnoreCase));
            var specifications = specificationsSheet is null
                ? []
                : ReadSpecifications(specificationsSheet, ReadHeaders(specificationsSheet));

            return Result.Success(new ProductExcelImportWorkbook(products, specifications));
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or ArgumentException)
        {
            return Result.Failure<ProductExcelImportWorkbook>("Cannot read Excel file");
        }
    }

    private static IReadOnlyCollection<ProductExcelImportProductRow> ReadProducts(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers)
    {
        var rows = new List<ProductExcelImportProductRow>();
        var lastRowNumber = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
        {
            if (IsBlankProductRow(sheet, headers, rowNumber))
                continue;

            var errors = new List<string>();
            var price = ReadDecimal(sheet, headers, rowNumber, "price", "price", errors);
            var stockQuantity = ReadInt(sheet, headers, rowNumber, "stockquantity", "stockQuantity", errors);
            var warrantyMonths = TryReadInt(sheet, headers, rowNumber, "warrantymonths", "warrantyMonths", errors);
            var isActive = TryReadBool(sheet, headers, rowNumber, "isactive", "isActive", errors);

            rows.Add(new ProductExcelImportProductRow(
                rowNumber,
                ReadText(sheet, headers, rowNumber, "sku"),
                ReadText(sheet, headers, rowNumber, "name"),
                ReadText(sheet, headers, rowNumber, "description"),
                ReadText(sheet, headers, rowNumber, "categorycode"),
                TryReadText(sheet, headers, rowNumber, "brand"),
                TryReadText(sheet, headers, rowNumber, "model"),
                warrantyMonths,
                price,
                stockQuantity,
                isActive,
                errors));
        }

        return rows;
    }

    private static IReadOnlyCollection<ProductExcelImportSpecificationRow> ReadSpecifications(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers)
    {
        if (!headers.ContainsKey("sku") || !headers.ContainsKey("key") || !headers.ContainsKey("value"))
        {
            return
            [
                new ProductExcelImportSpecificationRow(
                    1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    ["Specifications sheet must contain sku, key and value columns"])
            ];
        }

        var rows = new List<ProductExcelImportSpecificationRow>();
        var lastRowNumber = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
        {
            if (IsBlankSpecificationRow(sheet, headers, rowNumber))
                continue;

            var errors = new List<string>();
            var sku = ReadText(sheet, headers, rowNumber, "sku");
            var key = ReadText(sheet, headers, rowNumber, "key");
            var value = ReadText(sheet, headers, rowNumber, "value");

            if (string.IsNullOrWhiteSpace(sku))
                errors.Add("Specification SKU is required");
            if (string.IsNullOrWhiteSpace(key))
                errors.Add("Specification key is required");
            if (string.IsNullOrWhiteSpace(value))
                errors.Add("Specification value is required");

            rows.Add(new ProductExcelImportSpecificationRow(rowNumber, sku, key, value, errors));
        }

        return rows;
    }

    private static Dictionary<string, int> ReadHeaders(IXLWorksheet sheet)
    {
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastCellNumber = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;

        for (var columnNumber = 1; columnNumber <= lastCellNumber; columnNumber++)
        {
            var header = NormalizeHeader(sheet.Cell(1, columnNumber).GetFormattedString());
            if (!string.IsNullOrWhiteSpace(header) && !headers.ContainsKey(header))
                headers[header] = columnNumber;
        }

        return headers;
    }

    private static decimal? ReadDecimal(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header,
        string displayName,
        ICollection<string> errors)
    {
        var cell = Cell(sheet, headers, rowNumber, header);
        var text = cell.GetFormattedString().Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            errors.Add($"{displayName} is required");
            return null;
        }

        if (cell.TryGetValue<decimal>(out var typedValue))
            return typedValue;

        if (TryParseDecimal(text, out var parsedValue))
            return parsedValue;

        errors.Add($"{displayName} must be a number");
        return null;
    }

    private static int? ReadInt(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header,
        string displayName,
        ICollection<string> errors)
    {
        var cell = Cell(sheet, headers, rowNumber, header);
        var text = cell.GetFormattedString().Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            errors.Add($"{displayName} is required");
            return null;
        }

        if (cell.TryGetValue<int>(out var typedValue))
            return typedValue;

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
            return parsedValue;

        errors.Add($"{displayName} must be an integer");
        return null;
    }

    private static int? TryReadInt(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header,
        string displayName,
        ICollection<string> errors)
    {
        if (!headers.ContainsKey(header))
            return null;

        var cell = Cell(sheet, headers, rowNumber, header);
        var text = cell.GetFormattedString().Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (cell.TryGetValue<int>(out var typedValue))
            return typedValue;

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
            return parsedValue;

        errors.Add($"{displayName} must be an integer");
        return null;
    }

    private static bool? TryReadBool(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header,
        string displayName,
        ICollection<string> errors)
    {
        if (!headers.ContainsKey(header))
            return null;

        var cell = Cell(sheet, headers, rowNumber, header);
        var text = cell.GetFormattedString().Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (cell.TryGetValue<bool>(out var typedValue))
            return typedValue;

        var normalized = text.Trim().ToLowerInvariant();
        if (normalized is "1" or "yes" or "y" or "true" or "да")
            return true;
        if (normalized is "0" or "no" or "n" or "false" or "нет")
            return false;

        errors.Add($"{displayName} must be true or false");
        return null;
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        var cultures = new[] { CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("ru-RU") };
        foreach (var culture in cultures)
        {
            if (decimal.TryParse(text, NumberStyles.Number, culture, out value))
                return true;
        }

        value = 0m;
        return false;
    }

    private static string ReadText(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header) =>
        Cell(sheet, headers, rowNumber, header).GetFormattedString().Trim();

    private static string? TryReadText(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header) =>
        headers.ContainsKey(header) ? ReadText(sheet, headers, rowNumber, header) : null;

    private static IXLCell Cell(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber,
        string header) =>
        sheet.Cell(rowNumber, headers[header]);

    private static bool IsBlankProductRow(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber) =>
        RequiredProductHeaders.All(header =>
            string.IsNullOrWhiteSpace(Cell(sheet, headers, rowNumber, header).GetFormattedString()));

    private static bool IsBlankSpecificationRow(
        IXLWorksheet sheet,
        IReadOnlyDictionary<string, int> headers,
        int rowNumber) =>
        new[] { "sku", "key", "value" }
            .Where(headers.ContainsKey)
            .All(header => string.IsNullOrWhiteSpace(Cell(sheet, headers, rowNumber, header).GetFormattedString()));

    private static string NormalizeHeader(string header) =>
        new(header
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
}

public sealed record ProductExcelImportWorkbook(
    IReadOnlyCollection<ProductExcelImportProductRow> Products,
    IReadOnlyCollection<ProductExcelImportSpecificationRow> Specifications);

public sealed record ProductExcelImportProductRow(
    int RowNumber,
    string Sku,
    string Name,
    string Description,
    string CategoryCode,
    string? Brand,
    string? Model,
    int? WarrantyMonths,
    decimal? Price,
    int? StockQuantity,
    bool? IsActive,
    IReadOnlyCollection<string> Errors);

public sealed record ProductExcelImportSpecificationRow(
    int RowNumber,
    string Sku,
    string Key,
    string Value,
    IReadOnlyCollection<string> Errors);
