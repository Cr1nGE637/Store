using Microsoft.EntityFrameworkCore;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Entity;
using Store.Identity.Infrastructure.DbContexts;
using Store.Identity.Infrastructure.Entity;
using Store.Identity.Infrastructure.Services;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Inventory.Infrastructure.Entity;

namespace Store.App.Extensions;

public static class DemoDataExtensions
{
    public static async Task SeedDemoElectronicsAsync(this IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var catalog = provider.GetRequiredService<CatalogDbContext>();
        var identity = provider.GetRequiredService<IdentityDbContext>();
        var inventory = provider.GetRequiredService<InventoryDbContext>();

        await SeedDemoUsersAsync(identity, cancellationToken);

        var categories = DemoCategories();
        foreach (var category in categories)
        {
            if (!await catalog.Categories.AnyAsync(c => c.CategoryId == category.CategoryId, cancellationToken))
                catalog.Categories.Add(category);
        }

        await catalog.SaveChangesAsync(cancellationToken);

        var products = DemoProducts();
        foreach (var product in products)
        {
            if (!await catalog.Products.AnyAsync(p => p.Sku == product.Sku, cancellationToken))
                catalog.Products.Add(product);
        }

        await catalog.SaveChangesAsync(cancellationToken);

        foreach (var stock in DemoStock())
        {
            if (!await inventory.StockItems.AnyAsync(s => s.ProductId == stock.ProductId, cancellationToken))
                inventory.StockItems.Add(stock);
        }

        await inventory.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoUsersAsync(IdentityDbContext identity, CancellationToken cancellationToken)
    {
        var passwordHasher = new PasswordHasher();
        foreach (var user in DemoUsers(passwordHasher))
        {
            if (!await identity.Users.AnyAsync(u => u.Email == user.Email, cancellationToken))
                identity.Users.Add(user);
        }

        await identity.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<UserEntity> DemoUsers(PasswordHasher passwordHasher) =>
    [
        new()
        {
            Id = DemoIds.Customer,
            Name = "Demo Customer",
            Email = "customer@demo.local",
            PasswordHash = passwordHasher.Generate("Customer123!"),
            Role = "Customer"
        },
        new()
        {
            Id = DemoIds.Manager,
            Name = "Demo Manager",
            Email = "manager@demo.local",
            PasswordHash = passwordHasher.Generate("Manager123!"),
            Role = "Manager"
        }
    ];

    private static IReadOnlyList<CategoryEntity> DemoCategories() =>
    [
        new() { CategoryId = DemoIds.Smartphones, CategoryName = "Smartphones", CategoryCode = "Smartphone" },
        new() { CategoryId = DemoIds.Laptops, CategoryName = "Laptops", CategoryCode = "Laptop" },
        new() { CategoryId = DemoIds.Storage, CategoryName = "Storage", CategoryCode = "Storage" },
        new() { CategoryId = DemoIds.GraphicsCards, CategoryName = "Graphics Cards", CategoryCode = "GraphicsCard" },
        new() { CategoryId = DemoIds.Monitors, CategoryName = "Monitors", CategoryCode = "Monitor" },
        new() { CategoryId = DemoIds.Peripherals, CategoryName = "Peripherals", CategoryCode = "Peripheral" },
        new() { CategoryId = DemoIds.Accessories, CategoryName = "Accessories", CategoryCode = "Accessory" }
    ];

    private static IReadOnlyList<ProductEntity> DemoProducts() =>
    [
        Product(
            DemoIds.Iphone15,
            DemoIds.Smartphones,
            "APL-IP15-128-BLK",
            "Apple iPhone 15 128GB Black",
            "Smartphone with 6.1-inch OLED display, A16 Bionic and dual camera system.",
            79990m,
            "Apple",
            "iPhone 15",
            12,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Smartphone",
                ["Memory"] = "128GB",
                ["Display"] = "6.1 inch",
                ["Processor"] = "A16 Bionic",
                ["Connectivity"] = "5G"
            }),
        Product(
            DemoIds.GalaxyS24,
            DemoIds.Smartphones,
            "SMS-S24-256-GRY",
            "Samsung Galaxy S24 256GB Gray",
            "Compact Android smartphone with Dynamic AMOLED display and Galaxy AI features.",
            74990m,
            "Samsung",
            "Galaxy S24",
            12,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Smartphone",
                ["Memory"] = "256GB",
                ["Display"] = "6.2 inch",
                ["Processor"] = "Exynos 2400",
                ["Connectivity"] = "5G"
            }),
        Product(
            DemoIds.AsusZenbook,
            DemoIds.Laptops,
            "ASU-ZB14-OLED-U7",
            "ASUS Zenbook 14 OLED",
            "Lightweight notebook with OLED display, Intel Core Ultra 7 and 1TB SSD.",
            119990m,
            "ASUS",
            "Zenbook 14 OLED",
            24,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Laptop",
                ["Processor"] = "Intel Core Ultra 7",
                ["Memory"] = "16GB",
                ["Storage"] = "1TB SSD",
                ["Display"] = "14 inch OLED"
            }),
        Product(
            DemoIds.KingstonSsd,
            DemoIds.Storage,
            "KNG-KC3000-2TB",
            "Kingston KC3000 2TB NVMe SSD",
            "High-performance PCIe 4.0 NVMe SSD for gaming PCs and workstations.",
            15990m,
            "Kingston",
            "KC3000",
            60,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "SSD",
                ["Capacity"] = "2TB",
                ["Interface"] = "PCIe 4.0 NVMe",
                ["FormFactor"] = "M.2 2280"
            }),
        Product(
            DemoIds.Rtx4070,
            DemoIds.GraphicsCards,
            "MSI-RTX4070S-12G",
            "MSI GeForce RTX 4070 SUPER 12GB",
            "Graphics card for high-refresh 1440p gaming and CUDA workloads.",
            82990m,
            "MSI",
            "GeForce RTX 4070 SUPER",
            36,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "GraphicsCard",
                ["Memory"] = "12GB GDDR6X",
                ["Chipset"] = "AD104",
                ["Interface"] = "PCIe 4.0",
                ["Power"] = "220W"
            }),
        Product(
            DemoIds.LgMonitor,
            DemoIds.Monitors,
            "LG-27GP850-B",
            "LG UltraGear 27GP850-B",
            "27-inch QHD gaming monitor with Nano IPS panel and 165Hz refresh rate.",
            34990m,
            "LG",
            "27GP850-B",
            24,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Monitor",
                ["Display"] = "27 inch",
                ["Resolution"] = "2560x1440",
                ["RefreshRate"] = "165Hz",
                ["Panel"] = "Nano IPS"
            }),
        Product(
            DemoIds.LogitechKeyboard,
            DemoIds.Peripherals,
            "LOG-MXKEYS-S",
            "Logitech MX Keys S",
            "Wireless low-profile keyboard for productivity across multiple devices.",
            12990m,
            "Logitech",
            "MX Keys S",
            24,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Keyboard",
                ["ConnectionType"] = "Bluetooth",
                ["Layout"] = "Full-size",
                ["Backlight"] = "Yes"
            }),
        Product(
            DemoIds.AnkerCharger,
            DemoIds.Accessories,
            "ANK-735-65W",
            "Anker 735 Charger 65W",
            "Compact GaN charger with USB-C Power Delivery for phones and laptops.",
            5990m,
            "Anker",
            "735 Charger",
            18,
            new Dictionary<string, string>
            {
                ["DeviceType"] = "Charger",
                ["Power"] = "65W",
                ["Ports"] = "2xUSB-C,1xUSB-A",
                ["Technology"] = "GaN",
                ["Compatibility"] = "USB-C devices"
            })
    ];

    private static IReadOnlyList<StockItemEntity> DemoStock() =>
    [
        Stock(DemoIds.Iphone15, 12),
        Stock(DemoIds.GalaxyS24, 9),
        Stock(DemoIds.AsusZenbook, 5),
        Stock(DemoIds.KingstonSsd, 24),
        Stock(DemoIds.Rtx4070, 4),
        Stock(DemoIds.LgMonitor, 7),
        Stock(DemoIds.LogitechKeyboard, 18),
        Stock(DemoIds.AnkerCharger, 30)
    ];

    private static ProductEntity Product(
        Guid productId,
        Guid categoryId,
        string sku,
        string name,
        string description,
        decimal price,
        string brand,
        string model,
        int warrantyMonths,
        IReadOnlyDictionary<string, string> specifications) =>
        new()
        {
            ProductId = productId,
            CategoryId = categoryId,
            Sku = sku,
            ProductName = name,
            ProductDescription = description,
            ProductPrice = price,
            Brand = brand,
            Model = model,
            WarrantyMonths = warrantyMonths,
            Specifications = specifications
                .Select(s => new ProductSpecificationEntity
                {
                    ProductId = productId,
                    Name = s.Key,
                    Value = s.Value
                })
                .ToList()
        };

    private static StockItemEntity Stock(Guid productId, int quantity) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Reserved = 0
        };

    private static class DemoIds
    {
        public static readonly Guid Smartphones = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid Laptops = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid Storage = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid GraphicsCards = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid Monitors = Guid.Parse("10000000-0000-0000-0000-000000000005");
        public static readonly Guid Peripherals = Guid.Parse("10000000-0000-0000-0000-000000000006");
        public static readonly Guid Accessories = Guid.Parse("10000000-0000-0000-0000-000000000007");

        public static readonly Guid Customer = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid Manager = Guid.Parse("30000000-0000-0000-0000-000000000002");

        public static readonly Guid Iphone15 = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid GalaxyS24 = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid AsusZenbook = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid KingstonSsd = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid Rtx4070 = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public static readonly Guid LgMonitor = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public static readonly Guid LogitechKeyboard = Guid.Parse("20000000-0000-0000-0000-000000000007");
        public static readonly Guid AnkerCharger = Guid.Parse("20000000-0000-0000-0000-000000000008");
    }
}
