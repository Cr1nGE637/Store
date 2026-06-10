using CSharpFunctionalExtensions;
using Store.Carts.Application.CQRS.Command;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Domain.ValueObjects;

namespace Store.Tests.Application;

public class AddItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenGuestCartItemsArePostedConcurrently_CreatesOneCustomerCartAndMergesItems()
    {
        var customerId = Guid.NewGuid();
        var products = Enumerable.Range(1, 5)
            .Select(i => new ProductInfo(Guid.NewGuid(), $"Product {i}", 100m + i))
            .ToList();
        var cartRepository = new ConcurrentFakeCartRepository();
        var unitOfWork = new FakeCartUnitOfWork();
        var handler = new AddItemCommandHandler(
            cartRepository,
            new FakeProductCacheRepository(products),
            unitOfWork);

        var tasks = products.Select(product => handler.Handle(
            new AddItemCommand
            {
                CustomerId = customerId,
                ProductId = product.ProductId,
                Quantity = 1
            },
            CancellationToken.None));

        var results = await Task.WhenAll(tasks);

        Assert.All(results, result => Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty));
        Assert.Equal(1, cartRepository.AddCallCount);
        Assert.Equal(products.Count - 1, cartRepository.UpdateCallCount);
        Assert.Equal(products.Count, unitOfWork.SaveChangesCallCount);

        var cart = cartRepository.Cart;
        Assert.NotNull(cart);
        Assert.Equal(customerId, cart.CustomerId);
        Assert.Equal(products.Count, cart.Items.Count);
        Assert.All(products, product => Assert.Contains(cart.Items, item => item.ProductId == product.ProductId));
    }

    [Fact]
    public async Task Handle_WhenCartAppearsBetweenLookupAndCreate_ReloadsExistingCartAndAddsItem()
    {
        var customerId = Guid.NewGuid();
        var product = new ProductInfo(Guid.NewGuid(), "Keyboard", 99.9m);
        var cartRepository = new CartCreatedDuringAddRepository(customerId);
        var unitOfWork = new FakeCartUnitOfWork();
        var handler = new AddItemCommandHandler(
            cartRepository,
            new FakeProductCacheRepository([product]),
            unitOfWork);

        var result = await handler.Handle(
            new AddItemCommand
            {
                CustomerId = customerId,
                ProductId = product.ProductId,
                Quantity = 2
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
        Assert.Equal(1, cartRepository.AddCallCount);
        Assert.Equal(1, cartRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var cart = cartRepository.Cart;
        Assert.NotNull(cart);
        var item = Assert.Single(cart.Items);
        Assert.Equal(product.ProductId, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task Handle_WhenProductCacheHasMainImage_ReturnsCartItemImage()
    {
        var customerId = Guid.NewGuid();
        var product = new ProductInfo(
            Guid.NewGuid(),
            "Keyboard",
            99.9m,
            "/uploads/products/keyboard/image.jpg",
            "Keyboard front view");
        var handler = new AddItemCommandHandler(
            new ConcurrentFakeCartRepository(),
            new FakeProductCacheRepository([product]),
            new FakeCartUnitOfWork());

        var result = await handler.Handle(
            new AddItemCommand
            {
                CustomerId = customerId,
                ProductId = product.ProductId,
                Quantity = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
        var item = Assert.Single(result.Value.Items);
        Assert.NotNull(item.MainImage);
        Assert.Equal(product.MainImageUrl, item.MainImage.Url);
        Assert.Equal(product.MainImageAltText, item.MainImage.AltText);
    }


    private sealed class ConcurrentFakeCartRepository : ICartRepository
    {
        private readonly object _sync = new();
        private Cart? _cart;

        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }

        public Cart? Cart
        {
            get
            {
                lock (_sync)
                {
                    return _cart;
                }
            }
        }

        public Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId)
        {
            lock (_sync)
            {
                return Task.FromResult(_cart?.CustomerId == customerId
                    ? Result.Success(_cart)
                    : Result.Failure<Cart>("Cart not found"));
            }
        }

        public Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore) =>
            Task.FromResult<IReadOnlyList<Cart>>([]);

        public async Task<Result> AddAsync(Cart cart)
        {
            AddCallCount++;
            await Task.Delay(25);

            lock (_sync)
            {
                if (_cart?.CustomerId == cart.CustomerId)
                    return Result.Failure("Cart already exists for this customer");

                _cart = cart;
                return Result.Success();
            }
        }

        public Task<Result> UpdateAsync(Cart cart)
        {
            UpdateCallCount++;

            lock (_sync)
            {
                if (_cart?.CustomerId != cart.CustomerId)
                    return Task.FromResult(Result.Failure("Cart not found"));

                _cart = cart;
                return Task.FromResult(Result.Success());
            }
        }
    }

    private sealed class FakeProductCacheRepository(IEnumerable<ProductInfo> products) : IProductCacheRepository
    {
        private readonly Dictionary<Guid, ProductInfo> _products = products.ToDictionary(p => p.ProductId);

        public Task<Result<ProductInfo>> GetByIdAsync(Guid productId) =>
            Task.FromResult(_products.TryGetValue(productId, out var product)
                ? Result.Success(product)
                : Result.Failure<ProductInfo>("Product not found"));

        public Task<Result> AddAsync(ProductInfo product)
        {
            _products[product.ProductId] = product;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdatePriceAsync(Guid productId, decimal newPrice)
        {
            if (!_products.TryGetValue(productId, out var product))
                return Task.FromResult(Result.Failure("Product not found"));

            _products[productId] = product with { Price = newPrice };
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateMainImageAsync(Guid productId, string imageUrl, string altText)
        {
            if (!_products.TryGetValue(productId, out var product))
                return Task.FromResult(Result.Failure("Product not found"));

            _products[productId] = product with
            {
                MainImageUrl = imageUrl,
                MainImageAltText = altText
            };
            return Task.FromResult(Result.Success());
        }

        public Task SetUnavailableAsync(Guid productId)
        {
            _products.Remove(productId);
            return Task.CompletedTask;
        }
    }

    private sealed class CartCreatedDuringAddRepository(Guid customerId) : ICartRepository
    {
        private const string CartAlreadyExistsForCustomer = "Cart already exists for this customer";
        private Cart? _cart;

        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public Cart? Cart => _cart;

        public Task<Result<Cart>> GetByCustomerIdAsync(Guid requestedCustomerId) =>
            Task.FromResult(_cart?.CustomerId == requestedCustomerId
                ? Result.Success(_cart)
                : Result.Failure<Cart>("Cart not found"));

        public Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore) =>
            Task.FromResult<IReadOnlyList<Cart>>([]);

        public Task<Result> AddAsync(Cart cart)
        {
            AddCallCount++;
            _cart = Store.Carts.Domain.Aggregates.Cart.Create(customerId).Value;
            return Task.FromResult(Result.Failure(CartAlreadyExistsForCustomer));
        }

        public Task<Result> UpdateAsync(Cart cart)
        {
            UpdateCallCount++;
            _cart = cart;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeCartUnitOfWork : ICartUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
