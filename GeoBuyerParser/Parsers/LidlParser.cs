using GeoBuyerPromotion.Helpers;
using GeoBuyerPromotion.Models;
using HtmlAgilityPack;

namespace GeoBuyerPromotion.Parsers;

internal class LidlParser : IParser
{
    public readonly string defaultUrl = "https://www.lidl.pl";
    public List<Category> GetCategories(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var categories = GetCategoriesInternal(doc);
        return categories;
    }

    public List<Category> GetCategoriesInternal(HtmlDocument document)
    {
        var categoryListNodes = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'n-header__subnavigation-list')]");
        if (categoryListNodes == null)
            return new List<Category>();

        var categoryNodes = categoryListNodes.SelectNodes(".//li");

        return categoryNodes.Select(node =>
        {
            string name = "";
            string categoryUrl = "";

            // gets the node of the category
            var categoryLinkNode = node.SelectSingleNode(".//a");
            if (categoryLinkNode != null)
            {
                categoryUrl = ParserHelper.RemoveMultipleSpaces(categoryLinkNode.Attributes.FirstOrDefault(x => x.Name == "href")?.Value);
            }
            var categoryNameNode = node.SelectSingleNode(".//span");
            if (categoryNameNode != null )
            {
                name = ParserHelper.RemoveMultipleSpaces(categoryNameNode.InnerHtml);
            }

            return new Category(Guid.NewGuid().ToString(), name, categoryUrl.Contains("https") ? categoryUrl : defaultUrl + categoryUrl);
        }).Where(x => x.categoryUrl != "https://www.lidl.pl/").ToList();
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
