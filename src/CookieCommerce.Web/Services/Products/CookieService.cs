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

    private static readonly List<FeaturedCookieDto> _favorites =
    [
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-111111111111"),
            Title = "Chocolate Chunk",
            Description = "Our signature cookie, loaded with huge chunks of 70% dark Belgian chocolate and a touch of Maldon sea salt.",
            Price = 4.50m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAeiqI9Umndonm4v80T6b-moERbzC1sTOw7Bkt2035Se6-qfrM2_DMYQQGNpX1q9ZXtCy4qnGBvK2ey2XDPfbCHUE5FlKo4g4qB1toFZlprrm0A4IxtLRGOpqWct-oqOHHFI_HoAeaDf6QXlhJo9ycpxhZ5EqjBcZBfXJGSCIHnPLSEWD6jz9J61WJCH9fPdC2d0sLrGnu0DKvFpXQYnSj5ke_4eG8wKYO_VYMXos8oKzwU2ztEV-PWsIX7qLV2ck1t6YoLjS5B2w",
            IsBestSeller = true,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            Title = "Matcha White Choc",
            Description = "Ceremonial grade Uji Matcha infused dough paired with creamy Belgian white chocolate pearls.",
            Price = 5.00m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuB1otY-IeURut3V7rXt8Wx3JsYP2JYHabwYcofJAN000-6XYjQK-ldkwp8RlBVB-hcZ05QjpB3OVGSeJvS36btsDg5DLjQQxJA6jIXEJAom1XWTs13VuOz5fcdlw4u1F-PJlWR_EUL8zkuykIAq2Y-S6Dk2px1fIUJ1Iz2R-9dauURifu8oEcilnb4TLGbJds7Q2-QViuolXWmJ6m6dUznDKJGuSNWO_X2Wh2QuJJRS-qrPmhTvleYkrYiKEQWfQdpnq5Vva_wcYA",
            IsBestSeller = false,
            IsLimitedEdition = true
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
            Title = "Tiger Stripe",
            Description = "Our namesake cookie. A swirl of charcoal cocoa dough and orange-zest Madagascar vanilla.",
            Price = 4.75m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAXTbkwJrJ9i2uSNY-6ivy7Id_ZXy61SfjAqYWdmIVkZBmr3Vm8wVqcI78dhf7Fv_1XB2rDlfsBsdvhAJjg6PNOu8dnBTGQ4fxgYagl_uIal69aByjCbq4htnJQdtACMFhc7KXD58W6C7w_94X2Iqmk1PfbwiUSwQu5NaRw2WlqaZ1Vw0z5pdl6SQHxkDRUWgUULNaSkBbV6OSqrfAVJP3vhLa1b9L9-66iRqgsQeYeJZGPUDMWhrNh84jNgxY76-dhg9DR1CLpHQ",
            IsBestSeller = false,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
            Title = "Velvet Tiger",
            Description = "Deep red cocoa dough with a molten cream cheese core and white chocolate drizzle.",
            Price = 5.25m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCB9uUxu-B8A9KuNSRgCGQzLicdBnYe1dE3sAn8nHUmQYGvZeQk2C1Z0EJj_Qa8o_NvZCVTktungpn9GxpIBhlBKX43wTBLXH918oSGngtZBh9BgMhBLY2gYMURr-8Z9Vzi8yAlFDFDXcn1czNu3kjkfmBc4tbSIyyPncwQN8D64jruWnnQaMbZNKMf1cEoiYAAfI_ED7G0NiuBDNs5VT9vL9WIyS6dzY9xyngI_FHtWElw8nyEcyJfUV7fA1OCokLFXLMwIl4Gkg",
            IsBestSeller = false,
            IsLimitedEdition = true
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-333333333333"),
            Title = "Caramel Sea Salt",
            Description = "Rich brown butter dough packed with homemade salted caramel chunks that stay gooey.",
            Price = 4.50m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAkRv6e69KE5zmgQaikDjAem1lDQ9kqYGJeviSg-mdhcdMYZ2F4md9v_89Jebe4C3rhyVL80LCeyQBzcU2oxozfzqfo1NJK9pvFp0s2Msn2wYH0W0-LrCuenODLcoQmOh7UupEddumk4kyGUJV3fuZ0wB0sEHytq80lCMlSAwiCaJmH86MaowQUmWouVYWW_q_ZljLFPosq7o2EyQw_6RiDBW2OJCrIg8fkJRKGRLEeq2MTdcClgiH7TuGaAMu7iRBRD7aOXt50fg",
            IsBestSeller = false,
            IsLimitedEdition = false
        },
        new FeaturedCookieDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-666666666666"),
            Title = "Hazelnut Praline",
            Description = "Roasted hazelnuts and a smooth praline filling, finished with a dark chocolate drizzle.",
            Price = 4.75m,
            ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAClYDcfzF8RkGpoxGiEPGs_yB2_wxhuAkZQJbkYy2nfV8lspbymSWuMam4IAZX6LWfpLJQXejMTTep4m_e01OLN8XHIFSJBpXtZH4LCoaX-olS5d2RheuLZlIkh4t7sII9WplfYn45q2HyyZKMB9YgGGxmOzx2usigZyyivASTZ6w4pvPSHYtMxzmI4P2xeQl9w6fdN3fTdhM0lMfHP2PCfa_hPggoLqzPOZ85jhj8vv182zOA19GQ-rvqdpF6-2h2e-bvMyfJbg",
            IsBestSeller = false,
            IsLimitedEdition = false
        }
    ];

    public Task<IReadOnlyList<FeaturedCookieDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FeaturedCookieDto>>(_featuredCookies);
    }

    public Task<IReadOnlyList<FeaturedCookieDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FeaturedCookieDto>>(_favorites);
    }
}
