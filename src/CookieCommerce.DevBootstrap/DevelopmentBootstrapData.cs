namespace CookieCommerce.DevBootstrap;

public static class DevelopmentBootstrapData
{
    public static IReadOnlyList<DevelopmentBootstrapCustomer> Customers { get; } =
    [
        new(
            Email: "admin@tiger.com",
            FirstName: "Tiger",
            LastName: "Baker",
            Password: "Admin123!",
            Role: "Admin",
            Addresses:
            [
                new("Home", "home", "Bakerstraat", "12", null, "Utrecht", "3511 AB", "Utrecht", "Netherlands", "+31 6 1000 0001", true),
                new("Bakery", "store", "Oudegracht", "84", "A", "Utrecht", "3511 AV", "Utrecht", "Netherlands", "+31 30 100 0002", false),
                new("Office", "business", "Stationsplein", "7", null, "Utrecht", "3511 ED", "Utrecht", "Netherlands", "+31 30 100 0003", false),
                new("Parents", "family", "Lange Nieuwstraat", "22", null, "Utrecht", "3512 PH", "Utrecht", "Netherlands", "+31 6 1000 0004", false)
            ]),
        new(
            Email: "customer@tiger.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "Customer123!",
            Role: "User",
            Addresses:
            [
                new("Home", "home", "Cookie Lane", "15", null, "Amsterdam", "1012 AB", "North Holland", "Netherlands", "+31 6 2000 0001", true),
                new("Work", "business", "Keizersgracht", "241", null, "Amsterdam", "1016 EA", "North Holland", "Netherlands", "+31 20 200 0002", false),
                new("Pickup", "location_on", "Damrak", "50", "B", "Amsterdam", "1012 LL", "North Holland", "Netherlands", "+31 20 200 0003", false),
                new("Family", "family_restroom", "Nieuwe Binnenweg", "118", null, "Rotterdam", "3015 BE", "South Holland", "Netherlands", "+31 10 200 0004", false)
            ])
    ];
}

public sealed record DevelopmentBootstrapCustomer(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Role,
    IReadOnlyList<DevelopmentBootstrapAddress> Addresses);

public sealed record DevelopmentBootstrapAddress(
    string Label,
    string? Icon,
    string Street,
    string HouseNumber,
    string? HouseNumberAddition,
    string City,
    string PostalCode,
    string Province,
    string Country,
    string Phone,
    bool IsDefault);
