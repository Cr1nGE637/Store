using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.Application.Services;

public class ProductsService
{
    private readonly IProductsRepository _productsRepository;

    public ProductsService(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task GetProductByNameAsync(string name)
    {
        await _productsRepository.GetByNameProduct(name);
    }
    
    
}