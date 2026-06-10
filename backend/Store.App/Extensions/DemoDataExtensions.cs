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
            var existing = await catalog.Categories.FirstOrDefaultAsync(
                c => c.CategoryId == category.CategoryId,
                cancellationToken);

            if (existing is null)
            {
                catalog.Categories.Add(category);
                continue;
            }

            existing.CategoryName = category.CategoryName;
            existing.CategoryCode = category.CategoryCode;
        }

        await catalog.SaveChangesAsync(cancellationToken);

        var products = DemoProducts();
        foreach (var product in products)
        {
            var existing = await catalog.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Sku == product.Sku, cancellationToken);

            if (existing is null)
            {
                catalog.Products.Add(product);
                continue;
            }

            ApplyDemoProduct(existing, product);
        }

        await catalog.SaveChangesAsync(cancellationToken);

        foreach (var stock in DemoStock())
        {
            var existing = await inventory.StockItems.FirstOrDefaultAsync(
                s => s.ProductId == stock.ProductId,
                cancellationToken);

            if (existing is null)
            {
                inventory.StockItems.Add(stock);
                continue;
            }

            existing.Quantity = Math.Max(existing.Quantity, stock.Quantity);
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
            Name = "Customer",
            Email = "customer@gmail.com",
            PasswordHash = passwordHasher.Generate("customer123"),
            Role = "Customer"
        },
        new()
        {
            Id = DemoIds.Manager,
            Name = "Manager",
            Email = "manager@gmail.com",
            PasswordHash = passwordHasher.Generate("manager123"),
            Role = "Manager"
        }
    ];

    private static IReadOnlyList<CategoryEntity> DemoCategories() =>
    [
        new() { CategoryId = DemoIds.Processors, CategoryName = "Processors", CategoryCode = "Processors" },
        new() { CategoryId = DemoIds.Motherboards, CategoryName = "Motherboards", CategoryCode = "Motherboards" },
        new() { CategoryId = DemoIds.Ram, CategoryName = "RAM", CategoryCode = "RAM" },
        new() { CategoryId = DemoIds.Ssd, CategoryName = "SSD", CategoryCode = "SSD" },
        new() { CategoryId = DemoIds.GraphicsCards, CategoryName = "Graphics Cards", CategoryCode = "GraphicsCards" },
        new() { CategoryId = DemoIds.PowerSupplies, CategoryName = "Power Supplies", CategoryCode = "PowerSupplies" },
        new() { CategoryId = DemoIds.Laptops, CategoryName = "Laptops", CategoryCode = "Laptops" },
        new() { CategoryId = DemoIds.Smartphones, CategoryName = "Smartphones", CategoryCode = "Smartphones" },
        new() { CategoryId = DemoIds.Tablets, CategoryName = "Tablets", CategoryCode = "Tablets" },
        new() { CategoryId = DemoIds.Televisions, CategoryName = "Televisions", CategoryCode = "Televisions" },
        new() { CategoryId = DemoIds.Headphones, CategoryName = "Headphones", CategoryCode = "Headphones" },
        new() { CategoryId = DemoIds.Chargers, CategoryName = "Chargers", CategoryCode = "Chargers" },
        new() { CategoryId = DemoIds.CablesAdapters, CategoryName = "Cables and Adapters", CategoryCode = "CablesAdapters" },
        new() { CategoryId = DemoIds.SmartWatches, CategoryName = "Smart Watches", CategoryCode = "SmartWatches" },
        new() { CategoryId = DemoIds.GameConsoles, CategoryName = "Game Consoles", CategoryCode = "GameConsoles" },
        new() { CategoryId = DemoIds.NetworkEquipment, CategoryName = "Network Equipment", CategoryCode = "NetworkEquipment" },
        new() { CategoryId = DemoIds.Accessories, CategoryName = "Accessories", CategoryCode = "Accessories" },
        new() { CategoryId = DemoIds.Peripherals, CategoryName = "Peripherals", CategoryCode = "Peripherals" },
        new() { CategoryId = DemoIds.Monitors, CategoryName = "Monitors", CategoryCode = "Monitors" }
    ];

    private static IReadOnlyList<ProductEntity> DemoProducts() =>
    [
        Product(
            DemoIds.Ryzen7600,
            DemoIds.Processors,
            "AMD-R5-7600",
            "AMD Ryzen 5 7600",
            "AM5 desktop processor for balanced gaming and productivity builds.",
            21990m,
            "AMD",
            "Ryzen 5 7600",
            36,
            new Dictionary<string, string>
            {
                ["socket"] = "AM5",
                ["chipset"] = "Zen 4",
                ["powerConsumptionWatts"] = "65",
                ["compatibilityGroup"] = "AM5-DDR5"
            }),
        Product(
            DemoIds.IntelI513400F,
            DemoIds.Processors,
            "INT-I5-13400F",
            "Intel Core i5-13400F",
            "LGA1700 processor intentionally included to demonstrate an incompatible CPU choice.",
            18990m,
            "Intel",
            "Core i5-13400F",
            36,
            new Dictionary<string, string>
            {
                ["socket"] = "LGA1700",
                ["chipset"] = "Raptor Lake",
                ["powerConsumptionWatts"] = "65",
                ["compatibilityGroup"] = "LGA1700-DDR4"
            }),
        Product(
            DemoIds.AsusB650Plus,
            DemoIds.Motherboards,
            "ASU-B650-PLUS",
            "ASUS TUF Gaming B650-Plus",
            "ATX AM5 motherboard with DDR5 memory support for the compatible demo build.",
            21990m,
            "ASUS",
            "TUF Gaming B650-Plus",
            36,
            new Dictionary<string, string>
            {
                ["socket"] = "AM5",
                ["memoryType"] = "DDR5",
                ["formFactor"] = "ATX",
                ["chipset"] = "B650",
                ["compatibilityGroup"] = "AM5-DDR5"
            }),
        Product(
            DemoIds.KingstonDdr5,
            DemoIds.Ram,
            "KNG-FURY-DDR5-32",
            "Kingston Fury Beast DDR5 32GB",
            "DDR5 memory kit compatible with the AM5 demo motherboard.",
            10990m,
            "Kingston",
            "Fury Beast DDR5 32GB",
            60,
            new Dictionary<string, string>
            {
                ["memoryType"] = "DDR5",
                ["capacity"] = "32GB",
                ["formFactor"] = "DIMM",
                ["compatibilityGroup"] = "AM5-DDR5"
            }),
        Product(
            DemoIds.CrucialDdr4,
            DemoIds.Ram,
            "CRU-DDR4-16",
            "Crucial DDR4 16GB",
            "DDR4 memory intentionally included as an incompatible option for the AM5 demo build.",
            3990m,
            "Crucial",
            "DDR4 16GB",
            60,
            new Dictionary<string, string>
            {
                ["memoryType"] = "DDR4",
                ["capacity"] = "16GB",
                ["formFactor"] = "DIMM",
                ["compatibilityGroup"] = "LGA1700-DDR4"
            }),
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
                ["deviceModel"] = "iPhone 15",
                ["storage"] = "128GB",
                ["screenSize"] = "6.1",
                ["batteryCapacityMah"] = "3349",
                ["connectorType"] = "USB-C",
                ["requiredChargerWatts"] = "20",
                ["operatingSystem"] = "iOS",
                ["compatibilityGroup"] = "USB-C"
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
                ["deviceModel"] = "Galaxy S24",
                ["storage"] = "256GB",
                ["screenSize"] = "6.2",
                ["batteryCapacityMah"] = "4000",
                ["connectorType"] = "USB-C",
                ["requiredChargerWatts"] = "25",
                ["operatingSystem"] = "Android",
                ["compatibilityGroup"] = "USB-C"
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
                ["deviceModel"] = "Zenbook 14 OLED",
                ["processor"] = "Intel Core Ultra 7",
                ["memory"] = "16GB",
                ["memoryType"] = "LPDDR5X",
                ["storage"] = "1TB SSD",
                ["screenSize"] = "14",
                ["capacity"] = "1TB",
                ["connectorType"] = "USB-C",
                ["requiredChargerWatts"] = "65",
                ["compatibilityGroup"] = "USB-C"
            }),
        Product(
            DemoIds.KingstonSsd,
            DemoIds.Ssd,
            "KNG-KC3000-2TB",
            "Kingston KC3000 2TB NVMe SSD",
            "High-performance PCIe 4.0 NVMe SSD for gaming PCs and workstations.",
            15990m,
            "Kingston",
            "KC3000",
            60,
            new Dictionary<string, string>
            {
                ["capacity"] = "2TB",
                ["interface"] = "PCIe 4.0 NVMe",
                ["formFactor"] = "M.2 2280",
                ["compatibilityGroup"] = "M2-NVME"
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
                ["chipset"] = "AD104",
                ["interface"] = "PCIe 4.0",
                ["powerConsumptionWatts"] = "220",
                ["recommendedPsuWatts"] = "650",
                ["connectorType"] = "16-pin"
            }),
        Product(
            DemoIds.CorsairRm750e,
            DemoIds.PowerSupplies,
            "COR-RM750E",
            "Corsair RM750e 750W",
            "ATX power supply with enough headroom for the compatible RTX 4070 SUPER build.",
            12990m,
            "Corsair",
            "RM750e",
            84,
            new Dictionary<string, string>
            {
                ["powerConsumptionWatts"] = "750",
                ["formFactor"] = "ATX",
                ["connectorType"] = "16-pin",
                ["compatibilityGroup"] = "ATX-750W"
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
                ["screenSize"] = "27",
                ["resolution"] = "2560x1440",
                ["refreshRate"] = "165Hz",
                ["interface"] = "HDMI 2.1",
                ["connectorType"] = "HDMI",
                ["deviceModel"] = "27GP850-B"
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
                ["deviceType"] = "Keyboard",
                ["interface"] = "Bluetooth",
                ["connectorType"] = "Bluetooth",
                ["deviceModel"] = "MX Keys S"
            }),
        Product(
            DemoIds.AnkerCharger,
            DemoIds.Chargers,
            "ANK-735-65W",
            "Anker 735 Charger 65W",
            "Compact GaN charger with USB-C Power Delivery for phones and laptops.",
            5990m,
            "Anker",
            "735 Charger",
            18,
            new Dictionary<string, string>
            {
                ["compatibilityGroup"] = "USB-C",
                ["connectorType"] = "USB-C",
                ["powerWatts"] = "65",
                ["fastChargingStandard"] = "USB PD",
                ["compatibleDeviceModel"] = "iPhone 15; Galaxy S24; Zenbook 14 OLED"
            }),
        Product(
            DemoIds.IpadAir,
            DemoIds.Tablets,
            "APL-IPAD-AIR-M2-128",
            "Apple iPad Air 11 M2 128GB",
            "Thin tablet with an 11-inch Liquid Retina display, M2 chip and USB-C connectivity.",
            74990m,
            "Apple",
            "iPad Air 11 M2",
            12,
            new Dictionary<string, string>
            {
                ["deviceModel"] = "iPad Air 11 M2",
                ["storage"] = "128GB",
                ["screenSize"] = "11",
                ["batteryCapacityMah"] = "7606",
                ["connectorType"] = "USB-C",
                ["requiredChargerWatts"] = "20",
                ["operatingSystem"] = "iPadOS",
                ["compatibilityGroup"] = "USB-C"
            }),
        Product(
            DemoIds.SamsungTv,
            DemoIds.Televisions,
            "SMS-QLED-55Q80D",
            "Samsung QLED 55 Q80D",
            "55-inch 4K Smart TV with 120Hz panel, HDMI connectivity and gaming features.",
            99990m,
            "Samsung",
            "Q80D",
            24,
            new Dictionary<string, string>
            {
                ["screenSize"] = "55",
                ["resolution"] = "4K",
                ["smartTv"] = "Tizen",
                ["refreshRate"] = "120Hz",
                ["connectorType"] = "HDMI",
                ["ports"] = "HDMI x4, USB x2"
            }),
        Product(
            DemoIds.SonyHeadphones,
            DemoIds.Headphones,
            "SNY-WH1000XM5-BLK",
            "Sony WH-1000XM5 Black",
            "Wireless noise-cancelling headphones with Bluetooth connectivity and long battery life.",
            34990m,
            "Sony",
            "WH-1000XM5",
            12,
            new Dictionary<string, string>
            {
                ["connectionType"] = "Bluetooth",
                ["connectorType"] = "USB-C",
                ["batteryLifeHours"] = "30",
                ["compatibilityGroup"] = "Bluetooth"
            }),
        Product(
            DemoIds.BaseusCharger,
            DemoIds.Chargers,
            "BAS-GAN5-65W",
            "Baseus GaN5 Pro 65W",
            "Compact USB-C GaN charger for smartphones, tablets and ultrabooks with USB Power Delivery.",
            3990m,
            "Baseus",
            "GaN5 Pro 65W",
            12,
            new Dictionary<string, string>
            {
                ["powerWatts"] = "65",
                ["connectorType"] = "USB-C",
                ["fastChargingStandard"] = "USB PD",
                ["compatibilityGroup"] = "USB-C"
            }),
        Product(
            DemoIds.UgreenCable,
            DemoIds.CablesAdapters,
            "UGR-USBC-HDMI-4K",
            "UGREEN USB-C to HDMI 4K Adapter",
            "USB-C to HDMI adapter for connecting laptops, tablets and phones to monitors or TVs.",
            2490m,
            "UGREEN",
            "USB-C to HDMI 4K",
            12,
            new Dictionary<string, string>
            {
                ["cableInputType"] = "USB-C",
                ["cableOutputType"] = "HDMI",
                ["interface"] = "HDMI 2.0",
                ["connectorType"] = "USB-C; HDMI",
                ["compatibilityGroup"] = "USB-C-HDMI"
            }),
        Product(
            DemoIds.AppleWatch,
            DemoIds.SmartWatches,
            "APL-WATCH-S9-45",
            "Apple Watch Series 9 45mm",
            "Smart watch with Always-On Retina display, health sensors and wireless charging.",
            45990m,
            "Apple",
            "Watch Series 9",
            12,
            new Dictionary<string, string>
            {
                ["deviceModel"] = "Apple Watch Series 9",
                ["screenSize"] = "1.9",
                ["batteryLifeHours"] = "18",
                ["connectorType"] = "Magnetic",
                ["operatingSystem"] = "watchOS"
            }),
        Product(
            DemoIds.Playstation5,
            DemoIds.GameConsoles,
            "SNY-PS5-SLIM-1TB",
            "Sony PlayStation 5 Slim 1TB",
            "Home game console with 1TB storage, 4K output and HDMI connectivity.",
            64990m,
            "Sony",
            "PlayStation 5 Slim",
            12,
            new Dictionary<string, string>
            {
                ["deviceModel"] = "PlayStation 5 Slim",
                ["storage"] = "1TB",
                ["resolution"] = "4K",
                ["connectorType"] = "HDMI",
                ["compatibilityGroup"] = "PS5"
            }),
        Product(
            DemoIds.TpLinkRouter,
            DemoIds.NetworkEquipment,
            "TPL-ARCHER-AX55",
            "TP-Link Archer AX55",
            "Wi-Fi 6 router with dual-band wireless connectivity and gigabit Ethernet ports.",
            7990m,
            "TP-Link",
            "Archer AX55",
            24,
            new Dictionary<string, string>
            {
                ["networkStandard"] = "Wi-Fi 6",
                ["frequencyBand"] = "2.4GHz; 5GHz",
                ["interface"] = "Ethernet",
                ["ports"] = "Gigabit Ethernet x4"
            }),
        Product(
            DemoIds.SpigenCase,
            DemoIds.Accessories,
            "SPG-IP15-CASE-BLK",
            "Spigen Liquid Air Case for iPhone 15",
            "Protective case designed for iPhone 15 with precise button and connector cutouts.",
            1990m,
            "Spigen",
            "Liquid Air iPhone 15",
            6,
            new Dictionary<string, string>
            {
                ["compatibilityGroup"] = "iPhone 15",
                ["connectorType"] = "None",
                ["compatibleDeviceModel"] = "iPhone 15",
                ["deviceType"] = "Case",
                ["color"] = "Black"
            })
    ];

    private static IReadOnlyList<StockItemEntity> DemoStock() =>
    [
        Stock(DemoIds.Ryzen7600, 8),
        Stock(DemoIds.IntelI513400F, 6),
        Stock(DemoIds.AsusB650Plus, 5),
        Stock(DemoIds.KingstonDdr5, 14),
        Stock(DemoIds.CrucialDdr4, 12),
        Stock(DemoIds.Iphone15, 12),
        Stock(DemoIds.GalaxyS24, 9),
        Stock(DemoIds.AsusZenbook, 5),
        Stock(DemoIds.KingstonSsd, 24),
        Stock(DemoIds.Rtx4070, 4),
        Stock(DemoIds.CorsairRm750e, 10),
        Stock(DemoIds.LgMonitor, 7),
        Stock(DemoIds.LogitechKeyboard, 18),
        Stock(DemoIds.AnkerCharger, 30),
        Stock(DemoIds.IpadAir, 8),
        Stock(DemoIds.SamsungTv, 4),
        Stock(DemoIds.SonyHeadphones, 11),
        Stock(DemoIds.BaseusCharger, 25),
        Stock(DemoIds.UgreenCable, 34),
        Stock(DemoIds.AppleWatch, 9),
        Stock(DemoIds.Playstation5, 6),
        Stock(DemoIds.TpLinkRouter, 13),
        Stock(DemoIds.SpigenCase, 40)
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

    private static void ApplyDemoProduct(ProductEntity existing, ProductEntity source)
    {
        existing.CategoryId = source.CategoryId;
        existing.ProductName = source.ProductName;
        existing.ProductDescription = source.ProductDescription;
        existing.ProductPrice = source.ProductPrice;
        existing.Brand = source.Brand;
        existing.Model = source.Model;
        existing.WarrantyMonths = source.WarrantyMonths;

        var desired = source.Specifications.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        var current = existing.Specifications.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var specification in existing.Specifications.ToList())
        {
            if (!desired.ContainsKey(specification.Name))
                existing.Specifications.Remove(specification);
        }

        foreach (var specification in desired.Values)
        {
            if (current.TryGetValue(specification.Name, out var existingSpecification))
            {
                existingSpecification.Value = specification.Value;
                continue;
            }

            existing.Specifications.Add(new ProductSpecificationEntity
            {
                ProductId = existing.ProductId,
                Name = specification.Name,
                Value = specification.Value
            });
        }
    }

    private static class DemoIds
    {
        public static readonly Guid Smartphones = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid Laptops = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid Ssd = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid GraphicsCards = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid Monitors = Guid.Parse("10000000-0000-0000-0000-000000000005");
        public static readonly Guid Peripherals = Guid.Parse("10000000-0000-0000-0000-000000000006");
        public static readonly Guid Accessories = Guid.Parse("10000000-0000-0000-0000-000000000007");
        public static readonly Guid Processors = Guid.Parse("10000000-0000-0000-0000-000000000008");
        public static readonly Guid Motherboards = Guid.Parse("10000000-0000-0000-0000-000000000009");
        public static readonly Guid Ram = Guid.Parse("10000000-0000-0000-0000-000000000010");
        public static readonly Guid PowerSupplies = Guid.Parse("10000000-0000-0000-0000-000000000011");
        public static readonly Guid Tablets = Guid.Parse("10000000-0000-0000-0000-000000000012");
        public static readonly Guid Televisions = Guid.Parse("10000000-0000-0000-0000-000000000013");
        public static readonly Guid Headphones = Guid.Parse("10000000-0000-0000-0000-000000000014");
        public static readonly Guid Chargers = Guid.Parse("10000000-0000-0000-0000-000000000015");
        public static readonly Guid CablesAdapters = Guid.Parse("10000000-0000-0000-0000-000000000016");
        public static readonly Guid SmartWatches = Guid.Parse("10000000-0000-0000-0000-000000000017");
        public static readonly Guid GameConsoles = Guid.Parse("10000000-0000-0000-0000-000000000018");
        public static readonly Guid NetworkEquipment = Guid.Parse("10000000-0000-0000-0000-000000000019");

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
        public static readonly Guid Ryzen7600 = Guid.Parse("20000000-0000-0000-0000-000000000009");
        public static readonly Guid IntelI513400F = Guid.Parse("20000000-0000-0000-0000-000000000010");
        public static readonly Guid AsusB650Plus = Guid.Parse("20000000-0000-0000-0000-000000000011");
        public static readonly Guid KingstonDdr5 = Guid.Parse("20000000-0000-0000-0000-000000000012");
        public static readonly Guid CrucialDdr4 = Guid.Parse("20000000-0000-0000-0000-000000000013");
        public static readonly Guid CorsairRm750e = Guid.Parse("20000000-0000-0000-0000-000000000014");
        public static readonly Guid IpadAir = Guid.Parse("20000000-0000-0000-0000-000000000015");
        public static readonly Guid SamsungTv = Guid.Parse("20000000-0000-0000-0000-000000000016");
        public static readonly Guid SonyHeadphones = Guid.Parse("20000000-0000-0000-0000-000000000017");
        public static readonly Guid BaseusCharger = Guid.Parse("20000000-0000-0000-0000-000000000018");
        public static readonly Guid UgreenCable = Guid.Parse("20000000-0000-0000-0000-000000000019");
        public static readonly Guid AppleWatch = Guid.Parse("20000000-0000-0000-0000-000000000020");
        public static readonly Guid Playstation5 = Guid.Parse("20000000-0000-0000-0000-000000000021");
        public static readonly Guid TpLinkRouter = Guid.Parse("20000000-0000-0000-0000-000000000022");
        public static readonly Guid SpigenCase = Guid.Parse("20000000-0000-0000-0000-000000000023");
    }
}
