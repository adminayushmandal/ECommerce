namespace Shared.Constants
{
    public static class Contracts
    {
        public const string ClaimType = "permission";
        public const string AdminCanPurge = nameof(AdminCanPurge);

        public static class Dashboard
        {
            public const string View = "dashboard:view";
        }

        public static class Categories
        {
            public const string View = "categories:view";
            public const string Create = "categories:create";
            public const string Update = "categories:update";
            public const string Delete = "categories:delete";
        }

        public static class Products
        {
            public const string View = "products:view";
            public const string Create = "products:create";
            public const string Update = "products:update";
            public const string Delete = "products:delete";
            public const string Publish = "products:publish";
        }

        public static class ProductVariants
        {
            public const string View = "product-variants:view";
            public const string Create = "product-variants:create";
            public const string Update = "product-variants:update";
            public const string Delete = "product-variants:delete";
        }

        public static class Inventory
        {
            public const string View = "inventory:view";
            public const string Adjust = "inventory:adjust";
            public const string Reserve = "inventory:reserve";
        }

        public static class Orders
        {
            public const string View = "orders:view";
            public const string Manage = "orders:manage";
            public const string Cancel = "orders:cancel";
            public const string Fulfill = "orders:fulfill";
            public const string Refund = "orders:refund";
            public const string ViewOwn = "orders:view:own";
            public const string CancelOwn = "orders:cancel:own";
            public const string ReturnOwn = "orders:return:own";
        }

        public static class Customers
        {
            public const string View = "customers:view";
            public const string Manage = "customers:manage";
        }

        public static class Stores
        {
            public const string View = "stores:view";
            public const string Create = "stores:create";
            public const string Update = "stores:update";
            public const string Delete = "stores:delete";
        }

        public static class Promotions
        {
            public const string View = "promotions:view";
            public const string Manage = "promotions:manage";
        }

        public static class Coupons
        {
            public const string View = "coupons:view";
            public const string Manage = "coupons:manage";
        }

        public static class Shipping
        {
            public const string View = "shipping:view";
            public const string Manage = "shipping:manage";
        }

        public static class Returns
        {
            public const string View = "returns:view";
            public const string Approve = "returns:approve";
            public const string CreateOwn = "returns:create:own";
            public const string ViewOwn = "returns:view:own";
        }

        public static class Payments
        {
            public const string View = "payments:view";
            public const string Capture = "payments:capture";
            public const string Refund = "payments:refund";
        }

        public static class Reviews
        {
            public const string View = "reviews:view";
            public const string Moderate = "reviews:moderate";
            public const string Create = "reviews:create";
            public const string UpdateOwn = "reviews:update:own";
            public const string DeleteOwn = "reviews:delete:own";
        }

        public static class Reports
        {
            public const string View = "reports:view";
        }

        public static class Content
        {
            public const string View = "content:view";
            public const string Manage = "content:manage";
        }

        public static class Users
        {
            public const string View = "users:view";
            public const string Create = "users:create";
            public const string Update = "users:update";
            public const string Delete = "users:delete";
        }

        public static class Roles
        {
            public const string View = "roles:view";
            public const string Manage = "roles:manage";
        }

        public static class Permissions
        {
            public const string Assign = "permissions:assign";
        }

        public static class Settings
        {
            public const string View = "settings:view";
            public const string Update = "settings:update";
        }

        public static class AuditLogs
        {
            public const string View = "audit-logs:view";
        }

        public static class Profile
        {
            public const string View = "profile:view";
            public const string Update = "profile:update";
        }

        public static class Addresses
        {
            public const string View = "addresses:view";
            public const string Create = "addresses:create";
            public const string Update = "addresses:update";
            public const string Delete = "addresses:delete";
        }

        public static class Cart
        {
            public const string View = "cart:view";
            public const string Manage = "cart:manage";
        }

        public static class Checkout
        {
            public const string Process = "checkout:process";
        }

        public static class Wishlist
        {
            public const string View = "wishlist:view";
            public const string Manage = "wishlist:manage";
        }

        public static readonly IReadOnlySet<string> All = CreateSet(
            AdminCanPurge,
            Dashboard.View,
            Categories.View,
            Categories.Create,
            Categories.Update,
            Categories.Delete,
            Products.View,
            Products.Create,
            Products.Update,
            Products.Delete,
            Products.Publish,
            ProductVariants.View,
            ProductVariants.Create,
            ProductVariants.Update,
            ProductVariants.Delete,
            Inventory.View,
            Inventory.Adjust,
            Inventory.Reserve,
            Orders.View,
            Orders.Manage,
            Orders.Cancel,
            Orders.Fulfill,
            Orders.Refund,
            Orders.ViewOwn,
            Orders.CancelOwn,
            Orders.ReturnOwn,
            Customers.View,
            Customers.Manage,
            Stores.View,
            Stores.Create,
            Stores.Update,
            Stores.Delete,
            Promotions.View,
            Promotions.Manage,
            Coupons.View,
            Coupons.Manage,
            Shipping.View,
            Shipping.Manage,
            Returns.View,
            Returns.Approve,
            Returns.CreateOwn,
            Returns.ViewOwn,
            Payments.View,
            Payments.Capture,
            Payments.Refund,
            Reviews.View,
            Reviews.Moderate,
            Reviews.Create,
            Reviews.UpdateOwn,
            Reviews.DeleteOwn,
            Reports.View,
            Content.View,
            Content.Manage,
            Users.View,
            Users.Create,
            Users.Update,
            Users.Delete,
            Roles.View,
            Roles.Manage,
            Permissions.Assign,
            Settings.View,
            Settings.Update,
            AuditLogs.View,
            Profile.View,
            Profile.Update,
            Addresses.View,
            Addresses.Create,
            Addresses.Update,
            Addresses.Delete,
            Cart.View,
            Cart.Manage,
            Checkout.Process,
            Wishlist.View,
            Wishlist.Manage);

        public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> DefaultsByRole =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                [ApplicationRoles.Administrator] = All,
                [ApplicationRoles.StoreManager] = CreateSet(
                    Dashboard.View,
                    Categories.View,
                    Categories.Create,
                    Categories.Update,
                    Categories.Delete,
                    Products.View,
                    Products.Create,
                    Products.Update,
                    Products.Delete,
                    Products.Publish,
                    ProductVariants.View,
                    ProductVariants.Create,
                    ProductVariants.Update,
                    ProductVariants.Delete,
                    Inventory.View,
                    Inventory.Adjust,
                    Inventory.Reserve,
                    Orders.View,
                    Orders.Manage,
                    Orders.Cancel,
                    Orders.Fulfill,
                    Orders.Refund,
                    Customers.View,
                    Customers.Manage,
                    Stores.View,
                    Stores.Update,
                    Promotions.View,
                    Promotions.Manage,
                    Coupons.View,
                    Coupons.Manage,
                    Shipping.View,
                    Shipping.Manage,
                    Returns.View,
                    Returns.Approve,
                    Payments.View,
                    Payments.Capture,
                    Payments.Refund,
                    Reviews.View,
                    Reviews.Moderate,
                    Reports.View,
                    Content.View,
                    Content.Manage,
                    Settings.View,
                    Settings.Update),
                [ApplicationRoles.Customer] = CreateSet(
                    Categories.View,
                    Products.View,
                    ProductVariants.View,
                    Stores.View,
                    Promotions.View,
                    Coupons.View,
                    Reviews.View,
                    Reviews.Create,
                    Reviews.UpdateOwn,
                    Reviews.DeleteOwn,
                    Content.View,
                    Profile.View,
                    Profile.Update,
                    Addresses.View,
                    Addresses.Create,
                    Addresses.Update,
                    Addresses.Delete,
                    Cart.View,
                    Cart.Manage,
                    Checkout.Process,
                    Orders.ViewOwn,
                    Orders.CancelOwn,
                    Orders.ReturnOwn,
                    Returns.CreateOwn,
                    Returns.ViewOwn,
                    Wishlist.View,
                    Wishlist.Manage)
            };

        public static IReadOnlySet<string> GetByRole(string roleName) =>
            DefaultsByRole.TryGetValue(roleName, out var contracts) ? contracts : Empty;

        private static readonly IReadOnlySet<string> Empty = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static IReadOnlySet<string> CreateSet(params string[] contracts) =>
            new HashSet<string>(contracts, StringComparer.OrdinalIgnoreCase);
    }
}
