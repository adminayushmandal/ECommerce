namespace Infrastructure.Data.SeedData;

internal static class StoreSeedData
{
    public static IReadOnlyList<StoreSeed> Stores { get; } =
    [
        new(
            "DIY-LDH-01",
            "Cloth Store Ludhiana",
            "Model Town Main Road",
            "Ludhiana",
            "Punjab",
            "India",
            "141002",
            30.900965,
            75.857275,
            "Near Feroze Gandhi Market"),
        new(
            "DIY-DIB-01",
            "Cloth Store Dibrugarh",
            "H S Road",
            "Dibrugarh",
            "Assam",
            "India",
            "786001",
            27.472833,
            94.911964,
            "Near Chowkidinghee"),
        new(
            "DIY-SHL-01",
            "Cloth Store Shillong",
            "Police Bazaar Main Road",
            "Shillong",
            "Meghalaya",
            "India",
            "793001",
            25.578773,
            91.893254,
            "Near Ward's Lake")
    ];
}
