namespace Backend.Models
{
    public class ItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public User? User { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class ItemResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CreatedByUsername { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}