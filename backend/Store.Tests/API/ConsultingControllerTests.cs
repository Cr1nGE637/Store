using System.Reflection;
using System.Security.Claims;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Consulting.API.Controllers;
using Store.Consulting.API.Requests;
using Store.Consulting.Application.CQRS.Query;
using Store.Consulting.Application.DTOs;

namespace Store.Tests.API;

public class ConsultingControllerTests
{
    [Fact]
    public async Task CheckCartCompatibility_WhenMediatorReturnsSuccess_ReturnsOkWithConsultationResult()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CheckCartCompatibilityQuery? capturedQuery = null;
        var consultation = Consultation(productId, "Ok");
        var controller = CreateController(
            request =>
            {
                capturedQuery = Assert.IsType<CheckCartCompatibilityQuery>(request);
                return Result.Success(consultation);
            },
            customerId);

        var response = await controller.CheckCartCompatibility(
            new CheckCartCompatibilityRequest { ProductIds = [productId] },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(consultation, ok.Value);
        Assert.NotNull(capturedQuery);
        Assert.Equal(customerId, capturedQuery.CustomerId);
        Assert.Equal([productId], capturedQuery.ProductIds);
    }

    [Fact]
    public async Task CheckCartCompatibility_WhenMediatorReturnsFailure_ReturnsBadRequestProblem()
    {
        var controller = CreateController(_ =>
            Result.Failure<ConsultationResultDto>("Products not found: demo-product"));

        var response = await controller.CheckCartCompatibility(
            new CheckCartCompatibilityRequest { ProductIds = [Guid.NewGuid()] },
            CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        var details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal("Products not found: demo-product", details.Detail);
    }

    [Fact]
    public void CheckCartCompatibility_RequiresAuthorizedUser()
    {
        var controllerAuthorize = typeof(ConsultingController)
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true);
        var method = typeof(ConsultingController).GetMethod(nameof(ConsultingController.CheckCartCompatibility));

        Assert.Contains(controllerAuthorize, attribute => string.IsNullOrWhiteSpace(attribute.Roles));
        Assert.NotNull(method);
        Assert.Empty(method.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true));
    }

    private static ConsultingController CreateController(
        Func<object, object> responseFactory,
        Guid? customerId = null)
    {
        var controller = new ConsultingController(new FakeMediator(responseFactory));
        var httpContext = new DefaultHttpContext();

        if (customerId.HasValue)
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, customerId.Value.ToString())],
                "TestAuth");

            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static ConsultationResultDto Consultation(Guid productId, string status) =>
        new(
            Guid.NewGuid(),
            status,
            [new ConsultationItemDto(productId, "Processors", new Dictionary<string, string> { ["socket"] = "AM5" })],
            [],
            [],
            DateTime.UtcNow);

    private sealed class FakeMediator(Func<object, object> responseFactory) : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            Task.FromResult((TResponse)responseFactory(request));

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            responseFactory(request);
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            Task.FromResult<object?>(responseFactory(request));

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification =>
            Task.CompletedTask;
    }
}
