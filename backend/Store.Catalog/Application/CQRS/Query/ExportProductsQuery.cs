using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public sealed class ExportProductsQuery : IRequest<Result<ProductExportFileDto>>;
