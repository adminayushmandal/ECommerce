namespace Infrastructure.Data.SeedData;

internal static class StoreSeedData
{
    public static IReadOnlyList<StoreSeed> Stores { get; } =
    [
        new(
            "DIY-LDH-01",
            "Diyush",
            "Model Town Main Road",
            "Ludhiana",
            "Punjab",
            "India",
            "141002",
            30.900965,
            75.857275,
            "Near Feroze Gandhi Market")
    ];
}
