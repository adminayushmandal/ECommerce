namespace Infrastructure.Data.SeedData;

internal static class CatalogSeedData
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
