namespace Backend.Models
{
    public class ItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Et item kan have mange ratings
        public List<RatingsModel> Ratings { get; set; } = new();

        // Beregner automatisk den gennemsnitlige rating
        public decimal AverageRating => Ratings.Any() ? Ratings.Average(r => r.Score) : 0;
    }
}
