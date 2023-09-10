namespace GeoBuyerParser.Models;

public record Newspaper(string id, string name, string newspaperCode, string spotId, string url, string imageUrl, string? validInfo = null, string? type = null, string? description = null);

