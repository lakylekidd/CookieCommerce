using CookieCommerce.Web.Models.Content;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticFaqList
{
    public static List<FaqCategoryDto> FaqData = new()
    {
        new FaqCategoryDto
        {
            Icon = "shopping_basket",
            Title = "Ordering & Payment",
            Items = new List<FaqCategoryItemDto>
            {
                new FaqCategoryItemDto
                {
                    Id = 1,
                    Description = "How do I place a bulk order for events?",
                    Content = "For orders larger than 5 dozen, please contact our catering team at events@tigervancookie.com. We require at least 7 days' notice for large event orders to ensure everything is baked to perfection."
                },
                new FaqCategoryItemDto
                {
                    Id = 2,
                    Description = "Which payment methods do you accept?",
                    Content = "We accept all major credit cards (Visa, Mastercard, American Express), Apple Pay, and Google Pay. All transactions are securely processed and encrypted."
                }
            }
        },
        new FaqCategoryDto
        {
            Icon = "local_shipping",
            Title = "Shipping",
            Items = new List<FaqCategoryItemDto>
            {
                new FaqCategoryItemDto
                {
                    Id = 3,
                    Description = "How long will it take for my cookies to arrive?",
                    Content = "We bake and ship within 48 hours of your order. Standard shipping typically takes 2-3 business days. We recommend selecting Express Shipping if you're in a hurry for that fresh-baked taste!"
                },
                new FaqCategoryItemDto
                {
                    Id = 4,
                    Description = "Do you ship internationally?",
                    Content = "Currently, we only ship within the continental United States and Canada to ensure our cookies reach you while they're still fresh and delicious."
                }
            }
        },
        new FaqCategoryDto
        {
            Icon = "nutrition",
            Title = "Ingredients & Allergens",
            Items = new List<FaqCategoryItemDto>
            {
                new FaqCategoryItemDto
                {
                    Id = 5,
                    Description = "Are there gluten-free or vegan options?",
                    Content = "Yes! We have a dedicated 'Wild & Free' line that includes both vegan and gluten-free cookies. Please note that while we take every precaution, all cookies are baked in a facility that handles wheat, dairy, and nuts."
                },
                new FaqCategoryItemDto
                {
                    Id = 6,
                    Description = "How should I store my cookies?",
                    Content = "For the best experience, keep them in an airtight container at room temperature for up to 5 days. For that 'just-out-of-the-oven' feel, pop them in the microwave for 10-15 seconds!"
                }
            }
        }
    };
}
