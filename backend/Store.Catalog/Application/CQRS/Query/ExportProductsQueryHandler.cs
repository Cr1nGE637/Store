using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Application.Services;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public sealed class ExportProductsQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IProductAvailabilityRepository availabilityRepository,
    ProductExcelExportBuilder exportBuilder)
    : IRequestHandler<ExportProductsQuery, Result<ProductExportFileDto>>
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<Result<ProductExportFileDto>> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var productsResult = await productRepository.GetAllAsync();
        if (productsResult.IsFailure)
            return Result.Failure<ProductExportFileDto>(productsResult.Error);

        var categoriesResult = await categoryRepository.GetAllAsync();
        if (categoriesResult.IsFailure)
            return Result.Failure<ProductExportFileDto>(categoriesResult.Error);

        var productIds = productsResult.Value.Select(product => product.ProductId).ToArray();
        var availability = await availabilityRepository.GetAvailableQuantitiesAsync(productIds, cancellationToken);
        var categoryCodes = categoriesResult.Value.ToDictionary(
            category => category.CategoryId,
            category => category.CategoryCode.ToString());

        var products = productsResult.Value
            .OrderBy(product => product.ProductName)
            .Select(product => new ProductExportRow(
                product.Sku.Value,
                product.ProductName,
                product.ProductDescription,
                categoryCodes.GetValueOrDefault(product.CategoryId, "Unknown"),
                product.Brand,
                product.Model,
                product.WarrantyMonths,
                product.ProductPrice.Amount,
                availability.GetValueOrDefault(product.ProductId),
                IsActive: true))
            .ToArray();

        var specifications = productsResult.Value
            .OrderBy(product => product.Sku.Value)
            .SelectMany(product => product.Specifications
                .OrderBy(specification => specification.Name)
                .Select(specification => new ProductSpecificationExportRow(
                    product.Sku.Value,
                    specification.Name,
                    specification.Value,
                    Unit: string.Empty)))
            .ToArray();

        var content = exportBuilder.Build(products, specifications);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        return Result.Success(new ProductExportFileDto(
            $"storefit-products-{timestamp}.xlsx",
            ContentType,
            content));
    }
}
