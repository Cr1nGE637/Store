using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Application.Interfaces;
using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.Application.Services;

public class ProductService //: IProductService
{
    /*
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<Product>>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return Result.Success(products);
    }

    public async Task<Result<Product>> GetProductById(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product == null ? Result.Failure<Product>("Product not found") : Result.Success(product);
    }

    public async Task<Result> AddProduct(CreateProductDto createProductDto)
    {
        var existingProduct = await _productRepository.GetByNameAsync(createProductDto.ProductName);
        if (existingProduct != null) 
            return Result.Failure("Product is already exists");
        
        var productResult = Product.Create(createProductDto.ProductName, createProductDto.ProductDescription, createProductDto.ProductPrice);
        if (productResult.IsFailure)
            return Result.Failure(productResult.Error);
        
        var product = productResult.Value;
        await _productRepository.AddAsync(product);
        return Result.Success();
    }

    public async Task<Result> DeleteProduct(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return Result.Failure("Product not found");
        await _productRepository.Delete(productId);
        return Result.Success();
    }

    public async Task<Result> UpdateProduct(GetProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(productDto.ProductId);
        if (product == null) return Result.Failure("Product not found");
        var updatedProduct =  await _productRepository.UpdateProduct(product);
        return Result.Success(updatedProduct);
    }
    */
}