using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using System.Security.Claims;

namespace Infrastructure.Data
{
    public static class ApplicationDbContextInitializer
    {
        public static async Task AddSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
            await initializer.InitializeMigrationAsync(app.Environment.IsDevelopment());
            await initializer.SeedAsync();
        }
    }

    internal sealed class DatabaseSeedingService(ApplicationDbContext context, ILogger<DatabaseSeedingService> logger, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        public async Task InitializeMigrationAsync(bool isDevelopment)
        {
            try
            {
                if (isDevelopment && context.Database.IsNpgsql())
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.EnsureCreatedAsync();
                }
                else if (!isDevelopment && context.Database.IsNpgsql())
                {
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during the migration process.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                foreach (var roleContract in Contracts.DefaultsByRole)
                {
                    await SeedRoleAsync(roleContract.Key, roleContract.Value);
                }

                await SeedAdminUserAsync();
                await SeedCatalogAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while seeding the data to database.");
                throw;
            }
        }

        async Task SeedRoleAsync(string roleName, IReadOnlySet<string> contracts)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
                ArgumentNullException.ThrowIfNull(contracts);

                var role = await roleManager.FindByNameAsync(roleName);

                if (role is null)
                {
                    role = new Role(roleName);
                    await EnsureSucceededAsync(
                        () => roleManager.CreateAsync(role),
                        $"An error occured while creating role {roleName}.");

                    logger.LogInformation("Role - {roleName} inserted successfully", roleName);
                }
                else
                {
                    logger.LogInformation("Role - {roleName} skipped, because already existed.", roleName);
                }

                await SeedRoleClaimsAsync(role, contracts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while seeding the {roleName}", roleName);
                throw;
            }
        }

        async Task SeedRoleClaimsAsync(Role role, IReadOnlySet<string> contracts)
        {
            ArgumentNullException.ThrowIfNull(role);
            ArgumentNullException.ThrowIfNull(contracts);

            var expectedContracts = contracts
                .Where(static contract => !string.IsNullOrWhiteSpace(contract))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (expectedContracts.Count == 0)
            {
                logger.LogWarning("Role - {roleName} skipped claim seeding because no contracts were configured.", role.Name);
                return;
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingContracts = existingClaims
                .Where(static claim => string.Equals(claim.Type, Contracts.ClaimType, StringComparison.OrdinalIgnoreCase))
                .Select(static claim => claim.Value)
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingContracts = expectedContracts.Except(existingContracts, StringComparer.OrdinalIgnoreCase).ToArray();

            if (missingContracts.Length == 0)
            {
                logger.LogInformation("Role - {roleName} claims skipped, because all configured contracts already exist.", role.Name);
                return;
            }

            foreach (var contract in missingContracts)
            {
                await EnsureSucceededAsync(
                    () => roleManager.AddClaimAsync(role, new Claim(Contracts.ClaimType, contract)),
                    $"An error occured while adding claim {contract} to role {role.Name ?? role.Id}.");
            }

            logger.LogInformation(
                "Role - {roleName} claims inserted successfully. Added {claimCount} claim(s).",
                role.Name,
                missingContracts.Length);
        }

        async Task SeedAdminUserAsync()
        {
            try
            {
                var user = await userManager.FindByNameAsync("ayushmandal@mandalarc.net");

                if (user is null)
                {
                    user = new User("Ayush Krishan Mandal", "ayushmandal@mandalarc.net");

                    await EnsureSucceededAsync(
                        () => userManager.CreateAsync(user, "Test@123"),
                        $"An error occured while creating admin user {user.UserName}.");

                    logger.LogInformation("Admin user inserted successfully");
                }
                else
                {
                    logger.LogInformation("Admin user skipped, because already existed.");
                }

                if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Administrator))
                {
                    await EnsureSucceededAsync(
                        () => userManager.AddToRoleAsync(user, ApplicationRoles.Administrator),
                        $"An error occured while assigning role {ApplicationRoles.Administrator} to admin user {user.UserName}.");

                    logger.LogInformation("Admin role assigned successfully to {userName}", user.UserName);
                }
                else
                {
                    logger.LogInformation("Admin role skipped for {userName}, because it already existed.", user.UserName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while seeding admin user");
                throw;
            }
        }

        async Task SeedCatalogAsync()
        {
            var categoriesBySlug = await context.Categories
                .AsNoTracking()
                .ToDictionaryAsync(category => category.Slug, StringComparer.OrdinalIgnoreCase);

            var categorySeeds = CatalogSeedData.Categories;
            var newCategories = new List<Category>();

            foreach (var seed in categorySeeds)
            {
                if (categoriesBySlug.ContainsKey(seed.Slug))
                {
                    continue;
                }

                var category = new Category(
                    seed.Name,
                    seed.Slug,
                    seed.Description,
                    CreatePlaceholderImageUrl(seed.Name, seed.Color, 1200, 900));

                newCategories.Add(category);
                categoriesBySlug[seed.Slug] = category;
            }

            if (newCategories.Count > 0)
            {
                await context.Categories.AddRangeAsync(newCategories);
                await context.SaveChangesAsync();
                logger.LogInformation("Inserted {categoryCount} catalog categories.", newCategories.Count);
            }
            else
            {
                logger.LogInformation("Catalog categories skipped because all seeded categories already exist.");
            }

            categoriesBySlug = await context.Categories
                .AsNoTracking()
                .ToDictionaryAsync(category => category.Slug, StringComparer.OrdinalIgnoreCase);

            var productsBySku = await context.Products
                .AsNoTracking()
                .ToDictionaryAsync(product => product.Sku, StringComparer.OrdinalIgnoreCase);

            var productSeeds = CatalogSeedData.Products;
            var newProducts = new List<Product>();

            foreach (var seed in productSeeds)
            {
                if (productsBySku.ContainsKey(seed.Sku))
                {
                    continue;
                }

                if (!categoriesBySlug.TryGetValue(seed.CategorySlug, out var category))
                {
                    throw new InvalidOperationException($"Cannot seed product {seed.Sku} because category '{seed.CategorySlug}' does not exist.");
                }

                var product = new Product(
                    category.Id,
                    seed.Sku,
                    seed.Name,
                    seed.Slug,
                    seed.Description,
                    seed.BasePrice,
                    CreatePlaceholderImageUrl(seed.Name, seed.Color, 1200, 1200));

                newProducts.Add(product);
                productsBySku[seed.Sku] = product;
            }

            if (newProducts.Count > 0)
            {
                await context.Products.AddRangeAsync(newProducts);
                await context.SaveChangesAsync();
                logger.LogInformation("Inserted {productCount} catalog products.", newProducts.Count);
            }
            else
            {
                logger.LogInformation("Catalog products skipped because all seeded products already exist.");
            }

            productsBySku = await context.Products
                .AsNoTracking()
                .ToDictionaryAsync(product => product.Sku, StringComparer.OrdinalIgnoreCase);

            var variantsBySku = await context.ProductVariants
                .AsNoTracking()
                .ToDictionaryAsync(variant => variant.Sku, StringComparer.OrdinalIgnoreCase);

            var newVariants = new List<ProductVariant>();

            foreach (var seed in productSeeds)
            {
                if (!productsBySku.TryGetValue(seed.Sku, out var product))
                {
                    throw new InvalidOperationException($"Cannot seed variants for product {seed.Sku} because the product was not found.");
                }

                foreach (var variantSeed in seed.Variants)
                {
                    if (variantsBySku.ContainsKey(variantSeed.Sku))
                    {
                        continue;
                    }

                    var variant = new ProductVariant(
                        product.Id,
                        variantSeed.Sku,
                        variantSeed.Name,
                        variantSeed.AttributeSummary,
                        CreatePlaceholderImageUrl($"{seed.Name} {variantSeed.Name}", variantSeed.Color, 1200, 1200),
                        variantSeed.PriceOverride);

                    newVariants.Add(variant);
                    variantsBySku[variantSeed.Sku] = variant;
                }
            }

            if (newVariants.Count > 0)
            {
                await context.ProductVariants.AddRangeAsync(newVariants);
                await context.SaveChangesAsync();
                logger.LogInformation("Inserted {variantCount} product variants.", newVariants.Count);
            }
            else
            {
                logger.LogInformation("Product variants skipped because all seeded variants already exist.");
            }
        }

        static string CreatePlaceholderImageUrl(string text, string backgroundColor, int width, int height)
        {
            var encodedText = Uri.EscapeDataString(text);
            return $"https://dummyjson.com/image/{width}x{height}/{backgroundColor}/ffffff?text={encodedText}";
        }

        async Task EnsureSucceededAsync(
            Func<Task<IdentityResult>> action,
            string errorMessage)
        {
            var result = await action();

            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));

            logger.LogError("{message} Identity errors: {errors}", errorMessage, errors);
            throw new InvalidOperationException($"{errorMessage} Identity errors: {errors}");
        }

        static class CatalogSeedData
        {
            public static IReadOnlyList<CategorySeed> Categories { get; } =
            [
                new("audio-wearables", "Audio & Wearables", "Bluetooth audio, smart wearables, and everyday listening gear for work and commuting.", "0f766e"),
                new("workspace-tech", "Workspace & Tech", "Desk upgrades and compact accessories for productive hybrid setups.", "1d4ed8"),
                new("home-kitchen", "Home & Kitchen", "Home goods and kitchen staples focused on practical daily use.", "b45309"),
                new("fitness-outdoors", "Fitness & Outdoors", "Training accessories and outdoor gear built for active routines.", "15803d"),
                new("beauty-self-care", "Beauty & Self Care", "Skincare, fragrance, and beauty essentials for quick personal care rituals.", "be185d"),
                new("travel-everyday-carry", "Travel & Everyday Carry", "Durable bags and portable essentials for regular travel.", "374151")
            ];

            public static IReadOnlyList<ProductSeed> Products { get; } =
            [
                new("audio-wearables", "AUD-1001", "aurora-wireless-earbuds", "Aurora Wireless Earbuds", "Compact wireless earbuds with clear call quality and a pocket-size charging case.", 79.00m, "0f766e",
                [
                    new("AUD-1001-BLK", "Midnight Black", "Color: Midnight Black",  "111827", null),
                    new("AUD-1001-WHT", "Cloud White", "Color: Cloud White", "94a3b8", 5.00m)
                ]),
                new("audio-wearables", "AUD-1002", "atlas-over-ear-headphones", "Atlas Over-Ear Headphones", "Noise-isolating over-ear headphones designed for long work sessions and flights.", 149.00m, "115e59",
                [
                    new("AUD-1002-BLK", "Graphite", "Color: Graphite", "1f2937", null),
                    new("AUD-1002-SND", "Sandstone", "Color: Sandstone", "a16207", 10.00m)
                ]),
                new("audio-wearables", "AUD-1003", "pulse-smartwatch", "Pulse Smartwatch", "A lightweight smartwatch with wellness tracking, notifications, and multi-day battery life.", 199.00m, "0f766e",
                [
                    new("AUD-1003-BLK", "Black Silicone", "Band: Black Silicone", "111827", null),
                    new("AUD-1003-SLV", "Silver Mesh", "Band: Silver Mesh", "64748b", 15.00m)
                ]),
                new("audio-wearables", "AUD-1004", "aerofit-smart-band", "AeroFit Smart Band", "Slim activity band with heart rate monitoring and water-resistant construction.", 59.00m, "134e4a",
                [
                    new("AUD-1004-LME", "Lime Sport", "Band: Lime Sport", "65a30d", null),
                    new("AUD-1004-NVY", "Navy Sport", "Band: Navy Sport", "1d4ed8", null)
                ]),

                new("workspace-tech", "TEC-2001", "nova-mechanical-keyboard", "Nova Mechanical Keyboard", "Hot-swappable mechanical keyboard with compact layout and tactile feedback.", 129.00m, "1d4ed8",
                [
                    new("TEC-2001-WHT", "Ice White", "Switches: Tactile, Case: Ice White", "cbd5e1", null),
                    new("TEC-2001-CHR", "Charcoal", "Switches: Linear, Case: Charcoal", "1f2937", 10.00m)
                ]),
                new("workspace-tech", "TEC-2002", "glide-ergonomic-mouse", "Glide Ergonomic Mouse", "Right-handed ergonomic mouse tuned for all-day comfort and precise tracking.", 69.00m, "2563eb",
                [
                    new("TEC-2002-BLK", "Matte Black", "Finish: Matte Black", "111827", null),
                    new("TEC-2002-SLV", "Soft Silver", "Finish: Soft Silver", "94a3b8", 5.00m)
                ]),
                new("workspace-tech", "TEC-2003", "horizon-laptop-stand", "Horizon Laptop Stand", "Foldable aluminum stand that raises laptops for improved posture and airflow.", 54.00m, "1e40af",
                [
                    new("TEC-2003-SLV", "Silver", "Finish: Silver", "94a3b8", null),
                    new("TEC-2003-BLU", "Ocean Blue", "Finish: Ocean Blue", "1d4ed8", 4.00m)
                ]),
                new("workspace-tech", "TEC-2004", "beam-usb-c-dock", "Beam USB-C Dock", "Seven-port USB-C dock with HDMI, Ethernet, and pass-through charging support.", 89.00m, "1e3a8a",
                [
                    new("TEC-2004-GRY", "Graphite", "Finish: Graphite", "374151", null),
                    new("TEC-2004-SLV", "Aluminum Silver", "Finish: Aluminum Silver", "cbd5e1", 6.00m)
                ]),

                new("home-kitchen", "HOM-3001", "ember-stainless-bottle", "Ember Stainless Bottle", "Double-wall insulated bottle that keeps drinks cold through daily commutes.", 34.00m, "b45309",
                [
                    new("HOM-3001-SGE", "Sage Green", "Color: Sage Green", "4d7c0f", null),
                    new("HOM-3001-BLK", "Obsidian", "Color: Obsidian", "111827", null)
                ]),
                new("home-kitchen", "HOM-3002", "sear-cast-iron-skillet", "Sear Cast Iron Skillet", "Pre-seasoned cast iron skillet sized for weeknight cooking and oven finishes.", 46.00m, "92400e",
                [
                    new("HOM-3002-10", "10 Inch", "Size: 10 inch", "78350f", null),
                    new("HOM-3002-12", "12 Inch", "Size: 12 inch", "b45309", 8.00m)
                ]),
                new("home-kitchen", "HOM-3003", "mist-aroma-diffuser", "Mist Aroma Diffuser", "Quiet aroma diffuser with timed mist modes and warm ambient lighting.", 39.00m, "a16207",
                [
                    new("HOM-3003-BCH", "Beech", "Finish: Beech", "ca8a04", null),
                    new("HOM-3003-WHT", "Ceramic White", "Finish: Ceramic White", "cbd5e1", 3.00m)
                ]),
                new("home-kitchen", "HOM-3004", "loom-cotton-sheet-set", "Loom Cotton Sheet Set", "Breathable cotton sheet set with a soft washed finish for everyday comfort.", 79.00m, "b45309",
                [
                    new("HOM-3004-QN", "Queen - Sand", "Size: Queen, Color: Sand", "a16207", null),
                    new("HOM-3004-KG", "King - Mist", "Size: King, Color: Mist", "94a3b8", 12.00m)
                ]),

                new("fitness-outdoors", "FIT-4001", "trek-trail-running-shoes", "Trek Trail Running Shoes", "Grip-focused trail runners with responsive cushioning and reinforced toe guards.", 119.00m, "15803d",
                [
                    new("FIT-4001-42", "Size 42 - Moss", "Size: 42, Color: Moss", "3f6212", null),
                    new("FIT-4001-44", "Size 44 - Slate", "Size: 44, Color: Slate", "334155", null)
                ]),
                new("fitness-outdoors", "FIT-4002", "summit-hiking-backpack", "Summit Hiking Backpack", "Weather-ready daypack with hydration sleeve and modular outer straps.", 109.00m, "166534",
                [
                    new("FIT-4002-20", "20L - Forest", "Capacity: 20L, Color: Forest", "166534", null),
                    new("FIT-4002-30", "30L - Canyon", "Capacity: 30L, Color: Canyon", "b45309", 14.00m)
                ]),
                new("fitness-outdoors", "FIT-4003", "core-yoga-mat", "Core Yoga Mat", "Dense non-slip yoga mat built for home practice, stretching, and recovery sessions.", 42.00m, "15803d",
                [
                    new("FIT-4003-OLV", "Olive", "Color: Olive", "4d7c0f", null),
                    new("FIT-4003-RSE", "Rose", "Color: Rose", "be185d", null)
                ]),
                new("fitness-outdoors", "FIT-4004", "apex-resistance-bands", "Apex Resistance Bands", "Five-band resistance set for mobility work, warmups, and strength sessions.", 29.00m, "14532d",
                [
                    new("FIT-4004-LHT", "Light Set", "Resistance: Light to Medium", "0f766e", null),
                    new("FIT-4004-HVY", "Heavy Set", "Resistance: Medium to Heavy", "1e3a8a", 4.00m)
                ]),

                new("beauty-self-care", "BEA-5001", "luma-vitamin-c-serum", "Luma Vitamin C Serum", "Brightening serum formulated for daily use with a lightweight quick-absorbing texture.", 27.00m, "be185d",
                [
                    new("BEA-5001-30", "30 ml", "Size: 30 ml", "be185d", null),
                    new("BEA-5001-50", "50 ml", "Size: 50 ml", "ec4899", 8.00m)
                ]),
                new("beauty-self-care", "BEA-5002", "velvet-matte-lip-kit", "Velvet Matte Lip Kit", "Long-wear lip kit pairing a matte liquid color with a matching liner.", 24.00m, "9d174d",
                [
                    new("BEA-5002-MAU", "Muted Mauve", "Shade: Muted Mauve", "be185d", null),
                    new("BEA-5002-BER", "Berry Noir", "Shade: Berry Noir", "831843", null)
                ]),
                new("beauty-self-care", "BEA-5003", "calm-scalp-massager", "Calm Scalp Massager", "Flexible silicone scalp massager designed for shower use and gentle exfoliation.", 16.00m, "db2777",
                [
                    new("BEA-5003-BLU", "Spa Blue", "Color: Spa Blue", "1d4ed8", null),
                    new("BEA-5003-PNK", "Soft Pink", "Color: Soft Pink", "ec4899", null)
                ]),
                new("beauty-self-care", "BEA-5004", "drift-cedar-cologne", "Drift Cedar Cologne", "Fresh cedar-forward fragrance with citrus opening notes and a dry wood base.", 58.00m, "a21caf",
                [
                    new("BEA-5004-50", "50 ml", "Size: 50 ml", "7e22ce", null),
                    new("BEA-5004-100", "100 ml", "Size: 100 ml", "6b21a8", 18.00m)
                ]),

                new("travel-everyday-carry", "TRV-6001", "compass-weekender-duffel", "Compass Weekender Duffel", "Structured weekender bag with padded handles and separate shoe compartment.", 119.00m, "374151",
                [
                    new("TRV-6001-OLV", "Olive Canvas", "Material: Olive Canvas", "4d7c0f", null),
                    new("TRV-6001-BLK", "Black Twill", "Material: Black Twill", "111827", 6.00m)
                ]),
                new("travel-everyday-carry", "TRV-6002", "harbor-carry-on-case", "Harbor Carry-On Case", "Hard-shell carry-on case with 360-degree wheels and interior compression straps.", 159.00m, "4b5563",
                [
                    new("TRV-6002-SLV", "Silver Shell", "Shell: Silver", "94a3b8", null),
                    new("TRV-6002-NVY", "Navy Shell", "Shell: Navy", "1e3a8a", 10.00m)
                ]),
                new("travel-everyday-carry", "TRV-6003", "slate-leather-wallet", "Slate Leather Wallet", "Slim leather wallet with quick-access card slots and reinforced stitching.", 49.00m, "1f2937",
                [
                    new("TRV-6003-ESP", "Espresso", "Leather: Espresso", "78350f", null),
                    new("TRV-6003-CHR", "Charcoal", "Leather: Charcoal", "1f2937", null)
                ]),
                new("travel-everyday-carry", "TRV-6004", "nimbus-travel-pillow", "Nimbus Travel Pillow", "Supportive memory foam pillow with washable cover for long-haul travel comfort.", 37.00m, "6b7280",
                [
                    new("TRV-6004-GRY", "Mist Grey", "Color: Mist Grey", "94a3b8", null),
                    new("TRV-6004-NVY", "Deep Navy", "Color: Deep Navy", "1e3a8a", null)
                ])
            ];
        }

        sealed record CategorySeed(string Slug, string Name, string Description, string Color);

        sealed record ProductSeed(
            string CategorySlug,
            string Sku,
            string Slug,
            string Name,
            string Description,
            decimal BasePrice,
            string Color,
            IReadOnlyList<ProductVariantSeed> Variants);

        sealed record ProductVariantSeed(
            string Sku,
            string Name,
            string AttributeSummary,
            string Color,
            decimal? PriceOverride);
    }
}
