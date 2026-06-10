using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Application.Services;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Domain.Services;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class ImportProductsCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICatalogUnitOfWork unitOfWork,
    ICatalogDomainEventOutbox outbox,
    ProductExcelImportPreviewReader reader)
    : IRequestHandler<ImportProductsCommand, Result<ProductImportResultDto>>
{
    public async Task<Result<ProductImportResultDto>> Handle(
        ImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var workbookResult = reader.Read(request.Content);
        if (workbookResult.IsFailure)
            return Result.Failure<ProductImportResultDto>(workbookResult.Error);

        var productsResult = await productRepository.GetAllAsync();
        if (productsResult.IsFailure)
            return Result.Failure<ProductImportResultDto>(productsResult.Error);

        var categoriesResult = await categoryRepository.GetAllAsync();
        if (categoriesResult.IsFailure)
            return Result.Failure<ProductImportResultDto>(categoriesResult.Error);

        var existingProducts = productsResult.Value.ToDictionary(
            product => product.Sku.Value,
            StringComparer.OrdinalIgnoreCase);
        var categories = categoriesResult.Value.ToDictionary(
            category => category.CategoryCode.ToString(),
            StringComparer.OrdinalIgnoreCase);
        var workbook = workbookResult.Value;
        var duplicateSkus = FindDuplicateSkus(workbook.Products);
        var specifications = BuildSpecifications(workbook.Specifications);
        var validationErrors = ValidateWorkbook(workbook, existingProducts, categories, specifications, duplicateSkus);
        if (validationErrors.Count > 0)
        {
            return Result.Failure<ProductImportResultDto>(
                $"Import file contains errors: {string.Join("; ", validationErrors.Take(8))}");
        }

        var createdCount = 0;
        var updatedCount = 0;
        var stockUpdates = new List<ProductImportStockUpdateDto>();
        var changedProducts = new List<Product>();

        foreach (var row in workbook.Products)
        {
            var sku = NormalizeSku(row.Sku);
            var category = categories[row.CategoryCode.Trim()];
            var rowSpecifications = specifications.GetValueOrDefault(sku)
                ?? new Dictionary<string, string>();

            if (existingProducts.TryGetValue(sku, out var existingProduct))
            {
                var updateResult = existingProduct.Update(
                    sku,
                    row.Name,
                    row.Description,
                    row.Price!.Value,
                    string.IsNullOrWhiteSpace(row.Brand) ? existingProduct.Brand : row.Brand.Trim(),
                    string.IsNullOrWhiteSpace(row.Model) ? existingProduct.Model : row.Model.Trim(),
                    row.WarrantyMonths ?? existingProduct.WarrantyMonths,
                    category.CategoryId,
                    rowSpecifications);
                if (updateResult.IsFailure)
                    return Result.Failure<ProductImportResultDto>(updateResult.Error);

                if (updateResult.Value)
                {
                    var updateRepositoryResult = await productRepository.UpdateAsync(existingProduct);
                    if (updateRepositoryResult.IsFailure)
                        return Result.Failure<ProductImportResultDto>(updateRepositoryResult.Error);

                    await outbox.AddAsync(existingProduct.DomainEvents, cancellationToken);
                    changedProducts.Add(existingProduct);
                    updatedCount++;
                }

                stockUpdates.Add(new ProductImportStockUpdateDto(existingProduct.ProductId, row.StockQuantity!.Value));
                continue;
            }

            var createResult = Product.Create(
                sku,
                row.Name,
                row.Description,
                row.Price!.Value,
                row.Brand!.Trim(),
                row.Model!.Trim(),
                row.WarrantyMonths!.Value,
                category.CategoryId,
                rowSpecifications);
            if (createResult.IsFailure)
                return Result.Failure<ProductImportResultDto>(createResult.Error);

            var product = createResult.Value;
            await productRepository.AddAsync(product);
            await outbox.AddAsync(product.DomainEvents, cancellationToken);
            changedProducts.Add(product);
            existingProducts[sku] = product;
            stockUpdates.Add(new ProductImportStockUpdateDto(product.ProductId, row.StockQuantity!.Value));
            createdCount++;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (CatalogUniqueConstraintViolationException)
        {
            return Result.Failure<ProductImportResultDto>("Product already exists");
        }

        foreach (var product in changedProducts)
            product.ClearDomainEvents();

        return Result.Success(new ProductImportResultDto(
            workbook.Products.Count,
            createdCount,
            updatedCount,
            stockUpdates));
    }

    private static IReadOnlyCollection<string> ValidateWorkbook(
        ProductExcelImportWorkbook workbook,
        IReadOnlyDictionary<string, Product> existingProducts,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> specifications,
        IReadOnlySet<string> duplicateSkus)
    {
        var productSkus = workbook.Products
            .Select(row => NormalizeSku(row.Sku))
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var errors = workbook.Specifications
            .SelectMany(row =>
            {
                var rowErrors = row.Errors.Select(error => $"Specifications row {row.RowNumber}: {error}").ToList();
                var sku = NormalizeSku(row.Sku);
                if (!string.IsNullOrWhiteSpace(sku) && !productSkus.Contains(sku))
                    rowErrors.Add($"Specifications row {row.RowNumber}: SKU '{sku}' is not present on Products sheet");

                return rowErrors;
            })
            .ToList();

        foreach (var row in workbook.Products)
            errors.AddRange(ValidateProductRow(row, existingProducts, categories, specifications, duplicateSkus));

        return errors.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyCollection<string> ValidateProductRow(
        ProductExcelImportProductRow row,
        IReadOnlyDictionary<string, Product> existingProducts,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> specifications,
        IReadOnlySet<string> duplicateSkus)
    {
        var errors = row.Errors.Select(error => $"Products row {row.RowNumber}: {error}").ToList();
        var skuResult = Sku.Create(row.Sku);
        var sku = skuResult.IsSuccess ? skuResult.Value.Value : NormalizeSku(row.Sku);
        if (skuResult.IsFailure)
            errors.Add($"Products row {row.RowNumber}: {skuResult.Error}");

        if (duplicateSkus.Contains(sku))
            errors.Add($"Products row {row.RowNumber}: Duplicate SKU in import file");

        if (string.IsNullOrWhiteSpace(row.Name))
            errors.Add($"Products row {row.RowNumber}: Product name is required");

        if (string.IsNullOrWhiteSpace(row.CategoryCode))
            errors.Add($"Products row {row.RowNumber}: categoryCode is required");

        if (row.Price is null)
            errors.Add($"Products row {row.RowNumber}: price is required");

        if (row.StockQuantity is null)
            errors.Add($"Products row {row.RowNumber}: stockQuantity is required");
        else if (row.StockQuantity < 0)
            errors.Add($"Products row {row.RowNumber}: stockQuantity cannot be negative");

        if (row.IsActive is false)
            errors.Add($"Products row {row.RowNumber}: Import of inactive products is not supported yet");

        var existingProduct = existingProducts.GetValueOrDefault(sku);
        var brand = string.IsNullOrWhiteSpace(row.Brand) ? existingProduct?.Brand ?? string.Empty : row.Brand.Trim();
        var model = string.IsNullOrWhiteSpace(row.Model) ? existingProduct?.Model ?? string.Empty : row.Model.Trim();
        var warrantyMonths = row.WarrantyMonths ?? existingProduct?.WarrantyMonths;

        if (string.IsNullOrWhiteSpace(row.CategoryCode)
            || !categories.TryGetValue(row.CategoryCode.Trim(), out var category)
            || row.Price is null
            || warrantyMonths is null)
        {
            if (!string.IsNullOrWhiteSpace(row.CategoryCode) && !categories.ContainsKey(row.CategoryCode.Trim()))
                errors.Add($"Products row {row.RowNumber}: Category code not found");
            if (warrantyMonths is null)
                errors.Add($"Products row {row.RowNumber}: warrantyMonths is required");

            return errors;
        }

        var rowSpecifications = specifications.GetValueOrDefault(sku)
            ?? new Dictionary<string, string>();
        var rulesResult = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            category.CategoryCode,
            rowSpecifications);
        if (rulesResult.IsFailure)
            errors.Add($"Products row {row.RowNumber}: {rulesResult.Error}");

        var productResult = Product.Create(
            sku,
            row.Name,
            row.Description,
            row.Price.Value,
            brand,
            model,
            warrantyMonths.Value,
            category.CategoryId,
            rowSpecifications);
        if (productResult.IsFailure)
            errors.Add($"Products row {row.RowNumber}: {productResult.Error}");

        return errors;
    }

    private static IReadOnlySet<string> FindDuplicateSkus(IReadOnlyCollection<ProductExcelImportProductRow> rows) =>
        rows
            .Select(row => NormalizeSku(row.Sku))
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .GroupBy(sku => sku, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

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

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();
}
