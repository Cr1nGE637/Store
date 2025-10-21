using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<Product>>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllProductsAsync();
        return Result.Success(products);
    }

    public async Task<Result<Product>> GetProductById(Guid productId)
    {
        var product = await _productRepository.GetProductById(productId);
        return product == null ? Result.Failure<Product>("Product not found") : Result.Success(product);
    }

    public async Task<Result> AddProduct(CreateProductRequest createProductRequest)
    {
        var existingProduct = await _productRepository.GetProductByName(createProductRequest.ProductName);
        if (existingProduct != null) 
            return Result.Failure("Product is already exists");
        var productResult = Product.Create(createProductRequest.ProductName, createProductRequest.ProductDescription, createProductRequest.ProductPrice);
        if (productResult.IsFailure)
            return Result.Failure(productResult.Error);
        var product = productResult.Value;
        await _productRepository.AddProduct(product);
        return Result.Success();
    }

    public async Task<Result> DeleteProduct(Guid productId)
    {
        var product = await _productRepository.GetProductById(productId);
        if (product == null) return Result.Failure("Product not found");
        await _productRepository.DeleteProduct(productId);
        return Result.Success();
    }

    public async Task<Result> UpdateProduct(UpdateProductRequest productRequest)
    {
        var product = await _productRepository.GetProductById(productRequest.ProductId);
        if (product == null) return Result.Failure("Product not found");
        var updatedProduct =  await _productRepository.UpdateProduct(product);
        return Result.Success(updatedProduct);
    }
}