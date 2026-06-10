using ClosedXML.Excel;
using Store.Catalog.Application.Services;

namespace Store.Tests.Application;

public class ProductExcelExportBuilderTests
{
    [Fact]
    public void Build_CreatesWorkbookWithProductsAndSpecificationsSheets()
    {
        var builder = new ProductExcelExportBuilder();

        var bytes = builder.Build(
            [
                new ProductExportRow(
                    "CPU-001",
                    "Ryzen & Core",
                    "Processor <demo>",
                    "Processors",
                    "AMD",
                    "Ryzen 5 7600",
                    36,
                    12990.5m,
                    7,
                    true)
            ],
            [
                new ProductSpecificationExportRow("CPU-001", "socket", "AM5", string.Empty)
            ]);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var productsSheet = workbook.Worksheet("Products");
        var specificationsSheet = workbook.Worksheet("Specifications");

        Assert.Equal("sku", productsSheet.Cell(1, 1).GetString());
        Assert.Equal("name", productsSheet.Cell(1, 2).GetString());
        Assert.Equal("description", productsSheet.Cell(1, 3).GetString());
        Assert.Equal("categoryCode", productsSheet.Cell(1, 4).GetString());
        Assert.Equal("brand", productsSheet.Cell(1, 5).GetString());
        Assert.Equal("model", productsSheet.Cell(1, 6).GetString());
        Assert.Equal("warrantyMonths", productsSheet.Cell(1, 7).GetString());
        Assert.Equal("price", productsSheet.Cell(1, 8).GetString());
        Assert.Equal("stockQuantity", productsSheet.Cell(1, 9).GetString());
        Assert.Equal("isActive", productsSheet.Cell(1, 10).GetString());

        Assert.Equal("CPU-001", productsSheet.Cell(2, 1).GetString());
        Assert.Equal("Ryzen & Core", productsSheet.Cell(2, 2).GetString());
        Assert.Equal("Processor <demo>", productsSheet.Cell(2, 3).GetString());
        Assert.Equal("Processors", productsSheet.Cell(2, 4).GetString());
        Assert.Equal("AMD", productsSheet.Cell(2, 5).GetString());
        Assert.Equal("Ryzen 5 7600", productsSheet.Cell(2, 6).GetString());
        Assert.Equal(36, productsSheet.Cell(2, 7).GetValue<int>());
        Assert.Equal(12990.5, productsSheet.Cell(2, 8).GetDouble(), precision: 2);
        Assert.Equal(7, productsSheet.Cell(2, 9).GetValue<int>());
        Assert.True(productsSheet.Cell(2, 10).GetValue<bool>());

        Assert.Equal("sku", specificationsSheet.Cell(1, 1).GetString());
        Assert.Equal("key", specificationsSheet.Cell(1, 2).GetString());
        Assert.Equal("value", specificationsSheet.Cell(1, 3).GetString());
        Assert.Equal("unit", specificationsSheet.Cell(1, 4).GetString());

        Assert.Equal("CPU-001", specificationsSheet.Cell(2, 1).GetString());
        Assert.Equal("socket", specificationsSheet.Cell(2, 2).GetString());
        Assert.Equal("AM5", specificationsSheet.Cell(2, 3).GetString());
        Assert.Equal(string.Empty, specificationsSheet.Cell(2, 4).GetString());
    }
}
