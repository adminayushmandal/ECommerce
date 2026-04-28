namespace Infrastructure.Data.SeedData;

internal static class CatalogSeedData
{
    public static IReadOnlyList<CategorySeed> Categories { get; } =
    [
        new("mens-clothing", "Men", "Shirts, trousers, jackets, and knits built for everyday menswear capsules.", "171717"),
        new("womens-clothing", "Women", "Dresses, tailoring, denim, and knit layers for polished daily dressing.", "b85532"),
        new("kids-clothing", "Kids", "Durable tees, dungarees, hoodies, jackets, and occasion pieces for growing wardrobes.", "3563b5")
    ];

    public static IReadOnlyList<ProductSeed> Products { get; } =
    [
        new("mens-clothing", "MCL-1001", "oxford-overshirt", "Oxford Overshirt", "A structured cotton overshirt that layers cleanly over tees or fine knits.", 69.00m, "e7e5df",
        [
            new("MCL-1001-M-WHT", "M / White", "Size: M, Color: White, Fit: Regular", "f8fafc", null),
            new("MCL-1001-L-SKY", "L / Sky Blue", "Size: L, Color: Sky Blue, Fit: Regular", "93c5fd", null),
            new("MCL-1001-XL-CHR", "XL / Charcoal", "Size: XL, Color: Charcoal, Fit: Regular", "374151", 4.00m)
        ]),
        new("mens-clothing", "MCL-1002", "linen-resort-shirt", "Linen Resort Shirt", "A breathable short-sleeve linen shirt cut for warm days and relaxed evenings.", 54.00m, "d8c7a4",
        [
            new("MCL-1002-S-SGE", "S / Sage", "Size: S, Color: Sage, Fabric: Linen blend", "87986a", null),
            new("MCL-1002-M-CRM", "M / Cream", "Size: M, Color: Cream, Fabric: Linen blend", "efe4cf", null),
            new("MCL-1002-L-TER", "L / Terracotta", "Size: L, Color: Terracotta, Fabric: Linen blend", "b85532", 3.00m)
        ]),
        new("mens-clothing", "MCL-1003", "stretch-chino-trouser", "Stretch Chino Trouser", "A slim tapered chino with comfortable stretch for office days and weekend plans.", 79.00m, "1f2937",
        [
            new("MCL-1003-32-NVY", "32 / Navy", "Waist: 32, Color: Navy, Fit: Slim taper", "1e3a8a", null),
            new("MCL-1003-34-STN", "34 / Stone", "Waist: 34, Color: Stone, Fit: Slim taper", "c4b59b", null),
            new("MCL-1003-36-OLV", "36 / Olive", "Waist: 36, Color: Olive, Fit: Slim taper", "4d5f35", 5.00m)
        ]),
        new("mens-clothing", "MCL-1004", "selvedge-denim-jacket", "Selvedge Denim Jacket", "A rigid denim layer with clean hardware, roomy pockets, and a boxy profile.", 119.00m, "263b6a",
        [
            new("MCL-1004-M-IND", "M / Indigo", "Size: M, Color: Indigo, Fit: Boxy", "263b6a", null),
            new("MCL-1004-L-BLK", "L / Washed Black", "Size: L, Color: Washed Black, Fit: Boxy", "27272a", null),
            new("MCL-1004-XL-ECR", "XL / Ecru", "Size: XL, Color: Ecru, Fit: Boxy", "e8dfc8", 8.00m)
        ]),
        new("mens-clothing", "MCL-1005", "merino-crew-knit", "Merino Crew Knit", "A lightweight merino crew neck that works as a soft base layer or standalone sweater.", 86.00m, "2f2f2f",
        [
            new("MCL-1005-M-OAT", "M / Oat", "Size: M, Color: Oat, Fabric: Merino blend", "d6c7aa", null),
            new("MCL-1005-L-NVY", "L / Navy", "Size: L, Color: Navy, Fabric: Merino blend", "1e3a8a", null),
            new("MCL-1005-XL-FOR", "XL / Forest", "Size: XL, Color: Forest, Fabric: Merino blend", "166534", 6.00m)
        ]),

        new("womens-clothing", "WCL-2001", "wrap-midi-dress", "Wrap Midi Dress", "A fluid wrap dress with adjustable waist ties and a soft drape for day-to-night styling.", 98.00m, "b85532",
        [
            new("WCL-2001-XS-DHL", "XS / Dahlia", "Size: XS, Color: Dahlia, Length: Midi", "be185d", null),
            new("WCL-2001-S-BLK", "S / Black", "Size: S, Color: Black, Length: Midi", "171717", null),
            new("WCL-2001-M-SGE", "M / Sage", "Size: M, Color: Sage, Length: Midi", "87986a", 7.00m)
        ]),
        new("womens-clothing", "WCL-2002", "wide-leg-linen-trouser", "Wide-Leg Linen Trouser", "High-rise linen trousers with a relaxed wide leg and clean front pleats.", 84.00m, "c4b59b",
        [
            new("WCL-2002-26-STN", "26 / Stone", "Waist: 26, Color: Stone, Fit: Wide leg", "c4b59b", null),
            new("WCL-2002-28-OLV", "28 / Olive", "Waist: 28, Color: Olive, Fit: Wide leg", "4d5f35", null),
            new("WCL-2002-30-BLK", "30 / Black", "Waist: 30, Color: Black, Fit: Wide leg", "171717", 5.00m)
        ]),
        new("womens-clothing", "WCL-2003", "ribbed-knit-top", "Ribbed Knit Top", "A close-fit ribbed top with a soft hand feel and easy tuck-in length.", 42.00m, "efe4cf",
        [
            new("WCL-2003-S-IVY", "S / Ivory", "Size: S, Color: Ivory, Fit: Close", "f5efe2", null),
            new("WCL-2003-M-TPE", "M / Taupe", "Size: M, Color: Taupe, Fit: Close", "9a8372", null),
            new("WCL-2003-L-COC", "L / Cocoa", "Size: L, Color: Cocoa, Fit: Close", "6f4e37", 2.00m)
        ]),
        new("womens-clothing", "WCL-2004", "cropped-denim-jacket", "Cropped Denim Jacket", "A cropped denim jacket with a clean collar, contrast stitching, and everyday weight.", 92.00m, "4b72a8",
        [
            new("WCL-2004-S-BLU", "S / Mid Blue", "Size: S, Color: Mid Blue, Fit: Cropped", "4b72a8", null),
            new("WCL-2004-M-ECR", "M / Ecru", "Size: M, Color: Ecru, Fit: Cropped", "e8dfc8", null),
            new("WCL-2004-L-CHR", "L / Charcoal", "Size: L, Color: Charcoal, Fit: Cropped", "374151", 6.00m)
        ]),
        new("womens-clothing", "WCL-2005", "tailored-waistcoat", "Tailored Waistcoat", "A sharp sleeveless waistcoat designed for matching trousers or layered denim looks.", 76.00m, "b85532",
        [
            new("WCL-2005-XS-BLK", "XS / Black", "Size: XS, Color: Black, Fit: Tailored", "171717", null),
            new("WCL-2005-S-CRM", "S / Cream", "Size: S, Color: Cream, Fit: Tailored", "efe4cf", null),
            new("WCL-2005-M-TER", "M / Terracotta", "Size: M, Color: Terracotta, Fit: Tailored", "b85532", 4.00m)
        ]),

        new("kids-clothing", "KCL-3001", "kids-graphic-tee-pack", "Kids Graphic Tee Pack", "A three-tee cotton pack with playful prints and soft ribbed necklines.", 32.00m, "3563b5",
        [
            new("KCL-3001-4Y-OCN", "4Y / Ocean", "Size: 4Y, Color: Ocean, Pack: 3 tees", "2563eb", null),
            new("KCL-3001-6Y-SUN", "6Y / Sun", "Size: 6Y, Color: Sun, Pack: 3 tees", "f2b84b", null),
            new("KCL-3001-8Y-FOR", "8Y / Forest", "Size: 8Y, Color: Forest, Pack: 3 tees", "166534", 3.00m)
        ]),
        new("kids-clothing", "KCL-3002", "kids-denim-dungaree", "Kids Denim Dungaree", "Durable denim dungarees with adjustable straps and roomy patch pockets.", 58.00m, "263b6a",
        [
            new("KCL-3002-4Y-IND", "4Y / Indigo", "Size: 4Y, Color: Indigo, Fit: Adjustable", "263b6a", null),
            new("KCL-3002-6Y-SKY", "6Y / Sky", "Size: 6Y, Color: Sky, Fit: Adjustable", "93c5fd", null),
            new("KCL-3002-8Y-ECR", "8Y / Ecru", "Size: 8Y, Color: Ecru, Fit: Adjustable", "e8dfc8", 4.00m)
        ]),
        new("kids-clothing", "KCL-3003", "kids-quilted-jacket", "Kids Quilted Jacket", "A lightweight quilted jacket with snap closure and soft jersey lining.", 74.00m, "9f2f2f",
        [
            new("KCL-3003-5Y-RED", "5Y / Red", "Size: 5Y, Color: Red, Warmth: Light", "b91c1c", null),
            new("KCL-3003-7Y-NVY", "7Y / Navy", "Size: 7Y, Color: Navy, Warmth: Light", "1e3a8a", null),
            new("KCL-3003-9Y-OLV", "9Y / Olive", "Size: 9Y, Color: Olive, Warmth: Light", "4d5f35", 5.00m)
        ]),
        new("kids-clothing", "KCL-3004", "kids-cotton-hoodie", "Kids Cotton Hoodie", "A brushed cotton hoodie with a relaxed fit, kangaroo pocket, and ribbed cuffs.", 44.00m, "8b7bb8",
        [
            new("KCL-3004-4Y-LAV", "4Y / Lavender", "Size: 4Y, Color: Lavender, Fabric: Brushed cotton", "8b7bb8", null),
            new("KCL-3004-6Y-MNT", "6Y / Mint", "Size: 6Y, Color: Mint, Fabric: Brushed cotton", "8fbfa1", null),
            new("KCL-3004-8Y-CHR", "8Y / Charcoal", "Size: 8Y, Color: Charcoal, Fabric: Brushed cotton", "374151", 3.00m)
        ]),
        new("kids-clothing", "KCL-3005", "kids-party-dress", "Kids Party Dress", "A soft occasion dress with a lined bodice, full skirt, and easy back closure.", 68.00m, "d9a0b5",
        [
            new("KCL-3005-5Y-ROS", "5Y / Rose", "Size: 5Y, Color: Rose, Length: Knee", "d9a0b5", null),
            new("KCL-3005-7Y-IVY", "7Y / Ivory", "Size: 7Y, Color: Ivory, Length: Knee", "f5efe2", null),
            new("KCL-3005-9Y-BER", "9Y / Berry", "Size: 9Y, Color: Berry, Length: Knee", "9d174d", 6.00m)
        ])
    ];
}
