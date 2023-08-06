using GeoBuyerPromotion.Models;
using HtmlAgilityPack;

namespace GeoBuyerPromotion.Parsers;

public interface IParser
{
	public List<Category> GetCategories(string file);
    List<Category> GetCategoriesInternal(HtmlDocument document);
    public List<Product> GetProductsByCategory(string file, string categoryName);
    public List<Product> GetProductsByCategoryInternal(HtmlDocument document);
}

