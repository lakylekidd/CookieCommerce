using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public class CookieService : ICookieService
{
    private static readonly List<FeaturedCookieDto> _featuredCookies =
    [
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-111111111111"),
            Title = "Chocolate Chunk",
            Description = "Loaded with 70% dark Belgian chocolate chunks and a gooey center.",
            Price = 4.50m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAeiqI9Umndonm4v80T6b-moERbzC1sTOw7Bkt2035Se6-qfrM2_DMYQQGNpX1q9ZXtCy4qnGBvK2ey2XDPfbCHUE5FlKo4g4qB1toFZlprrm0A4IxtLRGOpqWct-oqOHHFI_HoAeaDf6QXlhJo9ycpxhZ5EqjBcZBfXJGSCIHnPLSEWD6jz9J61WJCH9fPdC2d0sLrGnu0DKvFpXQYnSj5ke_4eG8wKYO_VYMXos8oKzwU2ztEV-PWsIX7qLV2ck1t6YoLjS5B2w",
            IsBestSeller = true,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
            Title = "Tiger Stripe",
            Description = "A marbled masterpiece of dark cocoa and orange zest vanilla dough.",
            Price = 4.75m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAXTbkwJrJ9i2uSNY-6ivy7Id_ZXy61SfjAqYWdmIVkZBmr3Vm8wVqcI78dhf7Fv_1XB2rDlfsBsdvhAJjg6PNOu8dnBTGQ4fxgYagl_uIal69aByjCbq4htnJQdtACMFhc7KXD58W6C7w_94X2Iqmk1PfbwiUSwQu5NaRw2WlqaZ1Vw0z5pdl6SQHxkDRUWgUULNaSkBbV6OSqrfAVJP3vhLa1b9L9-66iRqgsQeYeJZGPUDMWhrNh84jNgxY76-dhg9DR1CLpHQ",
            IsBestSeller = false,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-333333333333"),
            Title = "Caramel Sea Salt",
            Description = "Rich butterscotch dough with a molten caramel heart and sea salt flakes.",
            Price = 4.50m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAkRv6e69KE5zmgQaikDjAem1lDQ9kqYGJeviSg-mdhcdMYZ2F4md9v_89Jebe4C3rhyVL80LCeyQBzcU2oxozfzqfo1NJK9pvFp0s2Msn2wYH0W0-LrCuenODLcoQmOh7UupEddumk4kyGUJV3fuZ0wB0sEHytq80lCMlSAwiCaJmH86MaowQUmWouVYWW_q_ZljLFPosq7o2EyQw_6RiDBW2OJCrIg8fkJRKGRLEeq2MTdcClgiH7TuGaAMu7iRBRD7aOXt50fg",
            IsBestSeller = false,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            Title = "Matcha White Choc",
            Description = "Premium ceremonial matcha dough with sweet white chocolate chips.",
            Price = 5.00m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuB1otY-IeURut3V7rXt8Wx3JsYP2JYHabwYcofJAN000-6XYjQK-ldkwp8RlBVB-hcZ05QjpB3OVGSeJvS36btsDg5DLjQQxJA6jIXEJAom1XWTs13VuOz5fcdlw4u1F-PJlWR_EUL8zkuykIAq2Y-S6Dk2px1fIUJ1Iz2R-9dauURifu8oEcilnb4TLGbJds7Q2-QViuolXWmJ6m6dUznDKJGuSNWO_X2Wh2QuJJRS-qrPmhTvleYkrYiKEQWfQdpnq5Vva_wcYA",
            IsBestSeller = true,
            IsLimitedEdition = true
        }
    ];

    public Task<IReadOnlyList<FeaturedCookieDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FeaturedCookieDto>>(_featuredCookies);
    }
}
