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
                .SingleOrDefaultAsync(r => r.ItemId == model.ItemId && r.UserId == model.UserId);

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
            var itemRatings = await BuildRatingsResponseQuery()
                .Where(r => r.ItemId == itemId)
                .ToListAsync();
            if (!itemRatings.Any())
            {
                return NotFound("Ingen ratings fundet for dette item.");
            }

            return Ok(itemRatings);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRatingsForUser(string userId)
        {
            var userRatings = await BuildRatingsResponseQuery()
                .Where(r => r.UserId == userId)
                .ToListAsync();
            if (!userRatings.Any())
            {
                return NotFound("Ingen ratings fundet for denne bruger.");
            }

            return Ok(userRatings);
        }

        private IQueryable<RatingResponseDto> BuildRatingsResponseQuery()
        {
            return from rating in _context.Ratings
                   join item in _context.Items on rating.ItemId equals item.Id into itemGroup
                   from item in itemGroup.DefaultIfEmpty()
                   join user in _context.Users on rating.UserId equals user.ID into userGroup
                   from user in userGroup.DefaultIfEmpty()
                   select new RatingResponseDto
                   {
                       Id = rating.Id,
                       ItemId = rating.ItemId,
                       ItemName = item != null ? item.Name : "Ukendt item",
                       ItemImageUrl = item != null ? item.ImageUrl : string.Empty,
                       UserId = rating.UserId,
                       Username = user != null ? user.Username : "Ukendt bruger",
                       Score = rating.Score
                   };
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
