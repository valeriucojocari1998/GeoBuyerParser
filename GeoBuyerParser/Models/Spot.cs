namespace GeoBuyerPromotion.Models
{
    public record Spot(string id, string provider, string? name = null, string? latitude = null, string? longitude = null);
}
