#nullable disable
namespace CookieCommerce.Web.Models.Addresses;

public class PdokResponse
{
    public ResponseData Response { get; set; }

    public class ResponseData
    {
        public List<Doc> Docs { get; set; }
    }

    public class Doc
    {
        public string Id { get; set; }
        public string Straatnaam { get; set; }
        public string Woonplaatsnaam { get; set; }
        public string Weergavenaam { get; set; }
    }
}
