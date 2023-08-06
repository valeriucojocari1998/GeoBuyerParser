using GeoBuyerPromotion.Helpers;
using GeoBuyerPromotion.Models;
using HtmlAgilityPack;

namespace GeoBuyerPromotion.Parsers;

public record BiedronkaParser : IParser
{
    public List<Category> GetCategories(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var categories = GetCategoriesInternal(doc);
        return categories;
    }

    public List<Category> GetCategoriesInternal(HtmlDocument document)
    {
        var categoryNodes = document.DocumentNode.SelectNodes("//li[contains(@class, 'refinement-category__list')]");
        if (categoryNodes == null)
            return new List<Category>();

        return categoryNodes.Select(node =>
        {
            string name = "";
            string categoryUrl = "";

            // gets the node of the category
            var categoryNode = node.SelectSingleNode(".//a[contains(@class, 'refinement-category__link')]");
            if (categoryNode != null)
            {
                name = ParserHelper.RemoveNumberPart(categoryNode.InnerText);
                categoryUrl = ParserHelper.RemoveMultipleSpaces(categoryNode.Attributes.FirstOrDefault(x => x.Name == "href")?.Value);
            }

            return new Category(Guid.NewGuid().ToString(), name, categoryUrl);
        }).ToList();
    }

    public List<Product> GetProductsByCategory(string html, string categoryName)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var products = GetProductsByCategoryInternal(doc);
        return products;
    }


    public List<Product> GetProductsByCategoryInternal(HtmlDocument document)
    {
        var products = new List<Product>();

        var productNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'product-tile js-product-tile')]");
        if (productNodes == null)
            return products;

        return productNodes.Select(productNode =>
        {
            string name = "";
            string brand = "";
            string saleSpecification = "";
            decimal currentPrice = decimal.MinValue;
            decimal? oldPrice = null;
            string priceLabel = "";
            string imageUrl = "";
            // Parse name
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'product-tile__name product-tile__name--overflow')]");
            if (nameNode != null)
                name = ParserHelper.RemoveMultipleSpaces(nameNode.InnerText);

            var brandNode = productNode.SelectSingleNode(".//div[contains(@class, 'product-tile__name')]");
            if (brandNode != null)
                brand = ParserHelper.RemoveMultipleSpaces(brandNode.InnerText);

            var saleSpecificationNode = productNode.SelectSingleNode(".//div[contains(@class, 'badge-tile__item\n badge-tile__item--text')]");
            if (saleSpecificationNode != null)
                saleSpecification = ParserHelper.RemoveMultipleSpaces(saleSpecificationNode.InnerText);

            // Parse sale price
            var currentPriceNode = productNode.SelectSingleNode(".//div[contains(@class, 'price-tile__sales')]");
            if (currentPriceNode != null)
                currentPrice = ParserHelper.ParsePrice(currentPriceNode.InnerText);


            // Parse old price
            var oldPriceNode = productNode.SelectSingleNode(".//div[contains(@class, 'price-tile__standard')]");
            if (oldPriceNode != null)
                oldPrice = ParserHelper.ParsePrice(oldPriceNode.InnerText);


            var descriptionNode = productNode.SelectSingleNode(".//span[contains(@class, 'product-omnibus-price__label')]");
            if (descriptionNode != null)
                priceLabel = ParserHelper.RemoveMultipleSpaces(descriptionNode.InnerText);

            var imageNode = productNode.SelectSingleNode(".//picture[contains(@class, 'tile-image__container')]");
            if (imageNode != null)
            {
                var pictureNode = imageNode.SelectSingleNode(".//img");
                imageUrl = ParserHelper.RemoveMultipleSpaces(pictureNode.Attributes.FirstOrDefault(x => x.Name == "srcset")?.Value);

            }

            return new Product(
                id: Guid.NewGuid().ToString(),
                name: name,
                currentPrice: currentPrice,
                oldPrice: oldPrice,
                brand: brand,
                priceLabel: priceLabel,
                saleSpecification: saleSpecification,
                imageUrl: imageUrl);
        }).ToList();
    }
}