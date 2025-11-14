using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Products.Query;

public class GetProductsQuery : IRequest<Result<List<GetProductDto>>>
{
    
}