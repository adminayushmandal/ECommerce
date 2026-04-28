using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using System.Security.Claims;
using Application.Common.Caching;
using Infrastructure.Data.SeedData;
using Application.Common.Interfaces;

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

    internal sealed class DatabaseSeedingService(
        ApplicationDbContext context,
        ILogger<DatabaseSeedingService> logger,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IApplicationCache applicationCache)
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
                await SeedStoreNetworkAsync();
                await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, CancellationToken.None);
                await applicationCache.InvalidateRegionAsync(CacheRegions.Stores, CancellationToken.None);
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

            await RetireProductsOutsideClothingCatalogAsync(categorySeeds, productSeeds);
        }

        async Task RetireProductsOutsideClothingCatalogAsync(
            IReadOnlyList<CategorySeed> categorySeeds,
            IReadOnlyList<ProductSeed> productSeeds)
        {
            var seededCategorySlugs = categorySeeds
                .Select(category => category.Slug)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var seededProductSkus = productSeeds
                .Select(product => product.Sku)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var seededVariantSkus = productSeeds
                .SelectMany(product => product.Variants.Select(variant => variant.Sku))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var retiredCount = 0;

            foreach (var category in await context.Categories.Where(category => category.IsActive).ToListAsync())
            {
                if (seededCategorySlugs.Contains(category.Slug))
                {
                    continue;
                }

                category.SetActive(false);
                retiredCount++;
            }

            foreach (var product in await context.Products.Where(product => product.IsActive).ToListAsync())
            {
                if (seededProductSkus.Contains(product.Sku))
                {
                    continue;
                }

                product.SetActive(false);
                retiredCount++;
            }

            foreach (var variant in await context.ProductVariants.Where(variant => variant.IsActive).ToListAsync())
            {
                if (seededVariantSkus.Contains(variant.Sku))
                {
                    continue;
                }

                variant.SetActive(false);
                retiredCount++;
            }

            if (retiredCount == 0)
            {
                logger.LogInformation("No non-clothing catalog records needed to be retired.");
                return;
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Retired {retiredCount} non-clothing catalog record(s).", retiredCount);
        }

        async Task SeedStoreNetworkAsync()
        {
            var storesByCode = await context.Stores
                .ToDictionaryAsync(store => store.Code, StringComparer.OrdinalIgnoreCase);

            var seededStores = new List<Store>();
            var insertedStoreCount = 0;
            var updatedStoreCount = 0;

            foreach (var storeSeed in StoreSeedData.Stores)
            {
                if (!storesByCode.TryGetValue(storeSeed.Code, out var store))
                {
                    store = new Store(
                        storeSeed.Code,
                        storeSeed.Name,
                        storeSeed.AddressLine1,
                        storeSeed.City,
                        storeSeed.State,
                        storeSeed.Country,
                        storeSeed.PostalCode,
                        storeSeed.Latitude,
                        storeSeed.Longitude,
                        storeSeed.AddressLine2);

                    await context.Stores.AddAsync(store);
                    storesByCode[storeSeed.Code] = store;
                    insertedStoreCount++;
                }
                else
                {
                    store.UpdateDetails(
                        storeSeed.Code,
                        storeSeed.Name,
                        storeSeed.AddressLine1,
                        storeSeed.City,
                        storeSeed.State,
                        storeSeed.Country,
                        storeSeed.PostalCode,
                        storeSeed.Latitude,
                        storeSeed.Longitude,
                        storeSeed.AddressLine2);

                    store.SetActive(true);
                    updatedStoreCount++;
                }

                seededStores.Add(store);
            }

            if (insertedStoreCount > 0 || updatedStoreCount > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Seeded store network. Inserted {insertedStoreCount} store(s), updated {updatedStoreCount} store(s).",
                    insertedStoreCount,
                    updatedStoreCount);
            }
            else
            {
                logger.LogInformation("Store network skipped because all seeded stores already match the configured locations.");
            }

            var productsBySku = await context.Products
                .AsNoTracking()
                .ToDictionaryAsync(product => product.Sku, StringComparer.OrdinalIgnoreCase);

            var variantsBySku = await context.ProductVariants
                .AsNoTracking()
                .ToDictionaryAsync(variant => variant.Sku, StringComparer.OrdinalIgnoreCase);

            var newInventoryItems = new List<InventoryItem>();

            for (var storeIndex = 0; storeIndex < seededStores.Count; storeIndex++)
            {
                var store = seededStores[storeIndex];
                var existingInventoryKeys = await context.InventoryItems
                    .AsNoTracking()
                    .Where(inventoryItem => inventoryItem.StoreId == store.Id)
                    .Select(inventoryItem => new InventorySeedKey(inventoryItem.ProductId, inventoryItem.ProductVariantId))
                    .ToListAsync();

                var existingInventoryKeySet = existingInventoryKeys.ToHashSet();
                var variantSequence = 0;

                foreach (var productSeed in CatalogSeedData.Products)
                {
                    if (!productsBySku.TryGetValue(productSeed.Sku, out var product))
                    {
                        throw new InvalidOperationException($"Cannot seed inventory because product '{productSeed.Sku}' does not exist.");
                    }

                    if (productSeed.Variants.Count == 0)
                    {
                        var inventoryKey = new InventorySeedKey(product.Id, null);

                        if (!existingInventoryKeySet.Contains(inventoryKey))
                        {
                            var quantityOnHand = CalculateSeedQuantity(storeIndex, variantSequence);
                            newInventoryItems.Add(new InventoryItem(store.Id, product.Id, null, quantityOnHand, 4));
                            existingInventoryKeySet.Add(inventoryKey);
                        }

                        variantSequence++;
                        continue;
                    }

                    foreach (var variantSeed in productSeed.Variants)
                    {
                        if (!variantsBySku.TryGetValue(variantSeed.Sku, out var variant))
                        {
                            throw new InvalidOperationException($"Cannot seed inventory because product variant '{variantSeed.Sku}' does not exist.");
                        }

                        var inventoryKey = new InventorySeedKey(product.Id, variant.Id);

                        if (existingInventoryKeySet.Contains(inventoryKey))
                        {
                            variantSequence++;
                            continue;
                        }

                        var quantityOnHand = CalculateSeedQuantity(storeIndex, variantSequence);
                        newInventoryItems.Add(new InventoryItem(store.Id, product.Id, variant.Id, quantityOnHand, 3));
                        existingInventoryKeySet.Add(inventoryKey);
                        variantSequence++;
                    }
                }
            }

            if (newInventoryItems.Count > 0)
            {
                await context.InventoryItems.AddRangeAsync(newInventoryItems);
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Inserted {inventoryCount} inventory records across {storeCount} store location(s).",
                    newInventoryItems.Count,
                    seededStores.Count);
            }
            else
            {
                logger.LogInformation("Inventory skipped because all seeded store and clothing variant records already exist.");
            }
        }

        static int CalculateSeedQuantity(int storeIndex, int variantSequence)
        {
            return 8 + ((storeIndex + 1) * 3) + ((variantSequence % 6) * 2);
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

    }
}
