using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class RatingsModel
    {
        public int Id { get; set; }

        // Kommer senere: ID på det item der bliver ratet
        public string ItemId { get; set; } = string.Empty;

        // Kommer senere: ID på brugeren der har givet ratingen
        public string UserId { get; set; } = string.Empty;

        // Rating på en 1-10 skala
        [Range(1, 10, ErrorMessage = "Rating skal være mellem 1 og 10.")]
        public int Score { get; set; }
    }
}
