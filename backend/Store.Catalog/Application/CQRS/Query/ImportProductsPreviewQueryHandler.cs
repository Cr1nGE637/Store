using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Services;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Domain.Services;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Application.CQRS.Query;

public sealed class ImportProductsPreviewQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ProductExcelImportPreviewReader reader)
    : IRequestHandler<ImportProductsPreviewQuery, Result<ProductImportPreviewDto>>
{
    public async Task<Result<ProductImportPreviewDto>> Handle(
        ImportProductsPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var workbookResult = reader.Read(request.Content);
        if (workbookResult.IsFailure)
            return Result.Failure<ProductImportPreviewDto>(workbookResult.Error);

        var productsResult = await productRepository.GetAllAsync();
        if (productsResult.IsFailure)
            return Result.Failure<ProductImportPreviewDto>(productsResult.Error);

        var categoriesResult = await categoryRepository.GetAllAsync();
        if (categoriesResult.IsFailure)
            return Result.Failure<ProductImportPreviewDto>(categoriesResult.Error);

        var existingProducts = productsResult.Value.ToDictionary(
            product => product.Sku.Value,
            StringComparer.OrdinalIgnoreCase);
        var categories = categoriesResult.Value.ToDictionary(
            category => category.CategoryCode.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var workbook = workbookResult.Value;
        var productSkus = workbook.Products
            .Select(row => NormalizeSku(row.Sku))
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var duplicateSkus = workbook.Products
            .Select(row => NormalizeSku(row.Sku))
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .GroupBy(sku => sku, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var specifications = BuildSpecifications(workbook.Specifications);
        var globalErrors = BuildGlobalSpecificationErrors(workbook.Specifications, productSkus);

        var rows = workbook.Products
            .Select(row => BuildPreviewRow(row, existingProducts, categories, specifications, duplicateSkus))
            .ToArray();

        var errorCount = rows.Count(row => row.Errors.Count > 0);
        return Result.Success(new ProductImportPreviewDto(
            rows.Length,
            rows.Count(row => row.Action == ProductImportPreviewActions.Create && row.Errors.Count == 0),
            rows.Count(row => row.Action == ProductImportPreviewActions.Update && row.Errors.Count == 0),
            errorCount,
            rows,
            globalErrors));
    }

    private static ProductImportPreviewRowDto BuildPreviewRow(
        ProductExcelImportProductRow row,
        IReadOnlyDictionary<string, Product> existingProducts,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> specifications,
        IReadOnlySet<string> duplicateSkus)
    {
        var errors = row.Errors.ToList();
        var skuResult = Sku.Create(row.Sku);
        var normalizedSku = skuResult.IsSuccess ? skuResult.Value.Value : NormalizeSku(row.Sku);
        if (skuResult.IsFailure)
            errors.Add(skuResult.Error);

        if (duplicateSkus.Contains(normalizedSku))
            errors.Add("Duplicate SKU in import file");

        if (string.IsNullOrWhiteSpace(row.Name))
            errors.Add("Product name is required");

        if (string.IsNullOrWhiteSpace(row.CategoryCode))
            errors.Add("categoryCode is required");

        if (row.Price is null)
            errors.Add("price is required");

        if (row.StockQuantity is null)
            errors.Add("stockQuantity is required");
        else if (row.StockQuantity < 0)
            errors.Add("stockQuantity cannot be negative");

        if (row.IsActive is false)
            errors.Add("Import of inactive products is not supported yet");

        var action = existingProducts.ContainsKey(normalizedSku)
            ? ProductImportPreviewActions.Update
            : ProductImportPreviewActions.Create;

        var existingProduct = existingProducts.GetValueOrDefault(normalizedSku);
        var brand = string.IsNullOrWhiteSpace(row.Brand) ? existingProduct?.Brand ?? string.Empty : row.Brand.Trim();
        var model = string.IsNullOrWhiteSpace(row.Model) ? existingProduct?.Model ?? string.Empty : row.Model.Trim();
        var warrantyMonths = row.WarrantyMonths ?? existingProduct?.WarrantyMonths;

        Category? category = null;
        if (!string.IsNullOrWhiteSpace(row.CategoryCode)
            && !categories.TryGetValue(row.CategoryCode.Trim(), out category))
        {
            errors.Add("Category code not found");
        }
        else if (row.Price is not null && warrantyMonths is not null && category is not null)
        {
            var rowSpecifications = specifications.GetValueOrDefault(normalizedSku)
                ?? new Dictionary<string, string>();
            var rulesResult = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
                category.CategoryCode,
                rowSpecifications);
            if (rulesResult.IsFailure)
                errors.Add(rulesResult.Error);

            var productResult = Product.Create(
                normalizedSku,
                row.Name,
                row.Description,
                row.Price.Value,
                brand,
                model,
                warrantyMonths.Value,
                category.CategoryId,
                rowSpecifications);
            if (productResult.IsFailure)
                errors.Add(productResult.Error);
        }
        else if (warrantyMonths is null)
        {
            errors.Add("warrantyMonths is required");
        }

        return new ProductImportPreviewRowDto(
            row.RowNumber,
            normalizedSku,
            string.IsNullOrWhiteSpace(row.Name) ? null : row.Name.Trim(),
            string.IsNullOrWhiteSpace(row.CategoryCode) ? null : row.CategoryCode.Trim(),
            row.Price,
            row.StockQuantity,
            errors.Count == 0 ? action : ProductImportPreviewActions.Error,
            errors.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> BuildSpecifications(
        IReadOnlyCollection<ProductExcelImportSpecificationRow> rows)
    {
        return rows
            .Where(row => row.Errors.Count == 0)
            .GroupBy(row => NormalizeSku(row.Sku), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyDictionary<string, string>)group
                    .GroupBy(row => row.Key.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(rowGroup => rowGroup.Key, rowGroup => rowGroup.First().Value.Trim(), StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyCollection<string> BuildGlobalSpecificationErrors(
        IReadOnlyCollection<ProductExcelImportSpecificationRow> rows,
        IReadOnlySet<string> productSkus)
    {
        return rows
            .SelectMany(row =>
            {
                var errors = row.Errors
                    .Select(error => $"Specifications row {row.RowNumber}: {error}")
                    .ToList();
                var sku = NormalizeSku(row.Sku);
                if (!string.IsNullOrWhiteSpace(sku) && !productSkus.Contains(sku))
                    errors.Add($"Specifications row {row.RowNumber}: SKU '{sku}' is not present on Products sheet");

                return errors;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();
}

public static class ProductImportPreviewActions
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Error = "Error";
}
