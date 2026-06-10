using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Catalog.API.Requests;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Application.CQRS.Query;
using Store.Catalog.Application.DTOs;


namespace Store.Catalog.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Manager")]
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateProductDto>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken token)
    {
        var command = new CreateProductCommand
        {
            Sku = request.Sku,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductPrice = request.ProductPrice,
            Brand = request.Brand,
            Model = request.Model,
            WarrantyMonths = request.WarrantyMonths,
            CategoryId = request.CategoryId,
            Specifications = request.Specifications
        };
        var result = await _mediator.Send(command, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<GetProductDto>>> GetAllProducts([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportProducts(CancellationToken token)
    {
        var result = await _mediator.Send(new ExportProductsQuery(), token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("import/preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductImportPreviewDto>> PreviewProductsImport(
        [FromForm] ImportProductsPreviewRequest request,
        CancellationToken token)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("Excel file is required");

        await using var stream = request.File.OpenReadStream();
        var result = await _mediator.Send(new ImportProductsPreviewQuery(stream, request.File.FileName), token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProductDto>> GetProductById(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(List<GetProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GetProductDto>>> GetProductsByCategory(
        Guid categoryId,
        CancellationToken token,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetProductsByCategoryQuery(categoryId, page, pageSize), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/images")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<ProductImageDto>>> GetProductImages(
        Guid id,
        CancellationToken token)
    {
        var result = await _mediator.Send(new GetProductImagesQuery { ProductId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductImageDto>> UploadProductImage(
        Guid id,
        [FromForm] UploadProductImageRequest request,
        CancellationToken token)
    {
        if (request.File is null)
            return BadRequest("Image file is required");

        await using var stream = request.File.OpenReadStream();
        var result = await _mediator.Send(new UploadProductImageCommand
        {
            ProductId = id,
            Content = stream,
            OriginalFileName = request.File.FileName,
            ContentType = request.File.ContentType,
            SizeBytes = request.File.Length,
            AltText = request.AltText,
            IsMain = request.IsMain
        }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}/images/{imageId:guid}/main")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductImageDto>> SetMainProductImage(
        Guid id,
        Guid imageId,
        CancellationToken token)
    {
        var result = await _mediator.Send(new SetMainProductImageCommand
        {
            ProductId = id,
            ProductImageId = imageId
        }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProductImage(
        Guid id,
        Guid imageId,
        CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteProductImageCommand
        {
            ProductId = id,
            ProductImageId = imageId
        }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken token)
    {
        var commandWithId = new UpdateProductCommand
        {
            ProductId = id,
            Sku = request.Sku,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductPrice = request.ProductPrice,
            Brand = request.Brand,
            Model = request.Model,
            WarrantyMonths = request.WarrantyMonths,
            CategoryId = request.CategoryId,
            Specifications = request.Specifications
        };
        var result = await _mediator.Send(commandWithId, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetProductDto>> DeleteProduct(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteProductCommand { ProductId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
