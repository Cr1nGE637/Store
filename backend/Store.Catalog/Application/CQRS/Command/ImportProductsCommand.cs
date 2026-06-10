using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public sealed record ImportProductsCommand(Stream Content, string FileName)
    : IRequest<Result<ProductImportResultDto>>;
