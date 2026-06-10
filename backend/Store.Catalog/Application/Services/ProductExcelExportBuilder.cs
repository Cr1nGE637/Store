using ClosedXML.Excel;

namespace Store.Catalog.Application.Services;

public sealed class ProductExcelExportBuilder
{
    public byte[] Build(IReadOnlyCollection<ProductExportRow> products, IReadOnlyCollection<ProductSpecificationExportRow> specifications)
    {
        using var workbook = new XLWorkbook();

        var productsSheet = workbook.Worksheets.Add("Products");
        WriteProductsSheet(productsSheet, products);

        var specificationsSheet = workbook.Worksheets.Add("Specifications");
        WriteSpecificationsSheet(specificationsSheet, specifications);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteProductsSheet(IXLWorksheet sheet, IReadOnlyCollection<ProductExportRow> products)
    {
        WriteProductsHeader(sheet);

        var row = 2;
        foreach (var product in products)
        {
            sheet.Cell(row, 1).Value = product.Sku;
            sheet.Cell(row, 2).Value = product.Name;
            sheet.Cell(row, 3).Value = product.Description;
            sheet.Cell(row, 4).Value = product.CategoryCode;
            sheet.Cell(row, 5).Value = product.Brand;
            sheet.Cell(row, 6).Value = product.Model;
            sheet.Cell(row, 7).Value = product.WarrantyMonths;
            sheet.Cell(row, 8).Value = product.Price;
            sheet.Cell(row, 9).Value = product.StockQuantity;
            sheet.Cell(row, 10).Value = product.IsActive;
            row++;
        }

        sheet.Column(8).Style.NumberFormat.Format = "#,##0.00";
        FormatSheet(sheet);
    }

    private static void WriteSpecificationsSheet(IXLWorksheet sheet, IReadOnlyCollection<ProductSpecificationExportRow> specifications)
    {
        WriteSpecificationsHeader(sheet);

        var row = 2;
        foreach (var specification in specifications)
        {
            sheet.Cell(row, 1).Value = specification.Sku;
            sheet.Cell(row, 2).Value = specification.Key;
            sheet.Cell(row, 3).Value = specification.Value;
            sheet.Cell(row, 4).Value = specification.Unit;
            row++;
        }

        FormatSheet(sheet);
    }

    private static void WriteProductsHeader(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "sku";
        sheet.Cell(1, 2).Value = "name";
        sheet.Cell(1, 3).Value = "description";
        sheet.Cell(1, 4).Value = "categoryCode";
        sheet.Cell(1, 5).Value = "brand";
        sheet.Cell(1, 6).Value = "model";
        sheet.Cell(1, 7).Value = "warrantyMonths";
        sheet.Cell(1, 8).Value = "price";
        sheet.Cell(1, 9).Value = "stockQuantity";
        sheet.Cell(1, 10).Value = "isActive";
    }

    private static void WriteSpecificationsHeader(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "sku";
        sheet.Cell(1, 2).Value = "key";
        sheet.Cell(1, 3).Value = "value";
        sheet.Cell(1, 4).Value = "unit";
    }

    private static void FormatSheet(IXLWorksheet sheet)
    {
        var usedRange = sheet.RangeUsed();
        if (usedRange is null)
        {
            return;
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);
        usedRange.SetAutoFilter();
        sheet.Columns().AdjustToContents();
    }
}

public sealed record ProductExportRow(
    string Sku,
    string Name,
    string Description,
    string CategoryCode,
    string Brand,
    string Model,
    int WarrantyMonths,
    decimal Price,
    int StockQuantity,
    bool IsActive);

public sealed record ProductSpecificationExportRow(
    string Sku,
    string Key,
    string Value,
    string Unit);
