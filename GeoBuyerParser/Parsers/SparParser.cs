using GeoBuyerPromotion.Helpers;
using GeoBuyerPromotion.Models;
using HtmlAgilityPack;

namespace GeoBuyerPromotion.Parsers;

public class SparParser: IParser
{
    public readonly string defaultUrl = "https://e-spar.com.pl";
    public List<Category> GetCategories(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var categories = GetCategoriesInternal(doc);
        return categories;
    }

    public List<Category> GetCategoriesInternal(HtmlDocument document)
    {
        var categoryListNode = document.DocumentNode.SelectSingleNode("//ul[contains(@class, 'clearfix')]");
        if (categoryListNode == null)
            return new List<Category>();
        var categoryNodes = categoryListNode.SelectNodes(".//a");

        return categoryNodes.Select(node =>
        {
            string name = "";
            string categoryUrl = "";

            // gets the node of the category
            name = ParserHelper.RemoveMultipleSpaces(node.InnerText);
            categoryUrl = ParserHelper.RemoveMultipleSpaces(node.Attributes.FirstOrDefault(x => x.Name == "href")?.Value);

            return new Category(Guid.NewGuid().ToString(), name, ParserHelper.NormalizeUrl(defaultUrl + categoryUrl));
        }).ToList();
    }

    public List<Product> GetProductsByCategory(string file, string categoryName)
    {
        return new List<Product>();
    }

    public List<Product> GetProductsByCategoryInternal(HtmlDocument document)
    {
        return new List<Product>();
    }
}
