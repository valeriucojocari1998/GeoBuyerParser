namespace GeoBuyerParser.Models;

public record NewspaperPage(string id, string page, string newspaperId, string pageUrl, string imageUrl, string dateCreated, string? newspaperCode = null);

