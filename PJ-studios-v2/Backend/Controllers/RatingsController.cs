using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddRating([FromBody] RatingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRating = await _context.Ratings
                .FromSqlInterpolated($@"
                    SELECT ""Id"", ""ItemId"", ""UserId"", ""Score""
                    FROM ""Ratings""
                    WHERE ""ItemId"" = {model.ItemId} AND ""UserId"" = {model.UserId}
                    LIMIT 1
                ")
                .SingleOrDefaultAsync();

            if (existingRating != null)
            {
                existingRating.Score = model.Score;
                await _context.SaveChangesAsync();

                return Ok(await BuildRatingResponseAsync(existingRating));
            }

            model.Id = 0;
            _context.Ratings.Add(model);
            await _context.SaveChangesAsync();

            return Ok(await BuildRatingResponseAsync(model));
        }

        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetRatingsForItem(string itemId)
        {
            var ratings = await _context.Ratings
                .FromSqlInterpolated($@"
                    SELECT ""Id"", ""ItemId"", ""UserId"", ""Score""
                    FROM ""Ratings""
                    WHERE ""ItemId"" = {itemId}
                ")
                .ToListAsync();

            if (!ratings.Any())
            {
                return NotFound("Ingen ratings fundet for dette item.");
            }

            var result = new List<RatingResponseDto>();
            foreach (var rating in ratings)
            {
                result.Add(await BuildRatingResponseAsync(rating));
            }

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRatingsForUser(string userId)
        {
            var ratings = await _context.Ratings
                .FromSqlInterpolated($@"
                    SELECT ""Id"", ""ItemId"", ""UserId"", ""Score""
                    FROM ""Ratings""
                    WHERE ""UserId"" = {userId}
                ")
                .ToListAsync();

            if (!ratings.Any())
            {
                return NotFound("Ingen ratings fundet for denne bruger.");
            }

            var result = new List<RatingResponseDto>();
            foreach (var rating in ratings)
            {
                result.Add(await BuildRatingResponseAsync(rating));
            }

            return Ok(result);
        }

        private async Task<RatingResponseDto> BuildRatingResponseAsync(RatingsModel rating)
        {
            var userName = await _context.Users
                .Where(u => u.ID == rating.UserId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();

            var item = await _context.Items
                .Where(i => i.Id == rating.ItemId)
                .Select(i => new { i.Name, i.ImageUrl })
                .SingleOrDefaultAsync();

            return new RatingResponseDto
            {
                Id = rating.Id,
                ItemId = rating.ItemId,
                ItemName = item?.Name ?? "Ukendt item",
                ItemImageUrl = item?.ImageUrl ?? string.Empty,
                UserId = rating.UserId,
                Username = userName ?? "Ukendt bruger",
                Score = rating.Score
            };
        }

        private sealed class RatingResponseDto
        {
            public int Id { get; set; }
            public string ItemId { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string ItemImageUrl { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public decimal Score { get; set; }
        }
    }
}