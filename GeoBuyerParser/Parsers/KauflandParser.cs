using GeoBuyerPromotion.Helpers;
using GeoBuyerPromotion.Models;
using HtmlAgilityPack;

namespace GeoBuyerPromotion.Parsers;

public class KauflandParser : IParser
{
    public readonly string defaultUrl = "https://www.kaufland.pl";
    public List<Category> GetCategories(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var categories = GetCategoriesInternal(doc);
        return categories;
    }

    public List<Category> GetCategoriesInternal(HtmlDocument document)
    {
        var categoryWrapperNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 't-offers-overview__categories')]");
        if (categoryWrapperNode == null)
            return new List<Category>();
        var categoryListNode = categoryWrapperNode.SelectSingleNode(".//li[contains(@class, 'm-accordion__item m-accordion__item--level-1')]");
        if (categoryListNode == null)
            return new List<Category>();
        var categoryNodes = categoryListNode.SelectNodes(".//li[contains(@class, 'm-accordion__item m-accordion__item--level-2')]");

        return categoryNodes.Select(node =>
        {
            string name = "";
            string categoryUrl = "";

            // gets the node of the category
            var categoryNode = node.SelectSingleNode(".//a[contains(@class, 'm-accordion__link')]");
            if (categoryNode != null)
            {
                name = ParserHelper.RemoveMultipleSpaces(categoryNode.InnerHtml);
                categoryUrl = ParserHelper.RemoveMultipleSpaces(categoryNode.Attributes.FirstOrDefault(x => x.Name == "href")?.Value);
            }

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
