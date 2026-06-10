using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.App.API.DTOs;
using Store.App.API.Requests;
using Store.App.Application;

namespace Store.App.API.Controllers;

[ApiController]
[Route("Products/import")]
public sealed class ProductImportsController(ProductImportOrchestrator orchestrator) : ControllerBase
{
    [Authorize(Roles = "Manager")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImportResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductImportResultResponse>> ImportProducts(
        [FromForm] ImportProductsRequest request,
        CancellationToken token)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("Excel file is required");

        await using var stream = request.File.OpenReadStream();
        var result = await orchestrator.ImportAsync(stream, request.File.FileName, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
