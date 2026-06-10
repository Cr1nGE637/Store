using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public sealed record ImportProductsPreviewQuery(Stream Content, string FileName)
    : IRequest<Result<ProductImportPreviewDto>>;
