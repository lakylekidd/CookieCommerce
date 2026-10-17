namespace CookieCommerce.DevBootstrap;

/// <summary>Reproducible fictional accounts; stable emails make startup seeding idempotent.</summary>
public static class DevelopmentDemoCustomers
{
    public static IReadOnlyList<DevelopmentBootstrapCustomer> Create()
    {
        var random = new Random(20261014);
        string[] firstNames = ["Noor", "Daan", "Sophie", "Milan", "Emma", "Finn", "Lotte", "Lucas"];
        string[] lastNames = ["de Vries", "Jansen", "Bakker", "Visser", "Smit", "Meijer", "de Boer", "Mulder"];
        string[] cities = ["Amsterdam", "Utrecht", "Rotterdam", "Haarlem"];
        var names = lastNames.OrderBy(_ => random.Next()).ToArray();
        return Enumerable.Range(0, 8).Select(i => new DevelopmentBootstrapCustomer(
            $"demo{i + 1:00}@cookiecommerce.example", firstNames[i], names[i], DevelopmentBootstrapData.DefaultPassword, "User",
            [new("Home", "home", "Voorbeeldstraat", (10 + i).ToString(), null, cities[i % cities.Length],
                "1012 AB", "Noord-Holland", "Netherlands", "+31 6 0000 0000", true)])).ToArray();
    }
}
