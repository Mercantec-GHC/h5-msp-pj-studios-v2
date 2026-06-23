using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await (
                from item in _context.Items
                join user in _context.Users on item.UserId equals user.ID into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new ItemResponseDto
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    CreatedByUsername = user != null ? user.Username : "Ukendt bruger",
                    Name = item.Name,
                    Description = item.Description,
                    ImageUrl = item.ImageUrl,
                    AverageRating = _context.Ratings
                        .Where(r => r.ItemId == item.Id)
                        .Select(r => (decimal?)r.Score)
                        .Average() ?? 0,
                    RatingCount = _context.Ratings.Count(r => r.ItemId == item.Id)
                }
            ).ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(string id)
        {
            var item = await (
                from i in _context.Items
                join user in _context.Users on i.UserId equals user.ID into userGroup
                from user in userGroup.DefaultIfEmpty()
                where i.Id == id
                select new ItemResponseDto
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    CreatedByUsername = user != null ? user.Username : "Ukendt bruger",
                    Name = i.Name,
                    Description = i.Description,
                    ImageUrl = i.ImageUrl,
                    AverageRating = _context.Ratings
                        .Where(r => r.ItemId == i.Id)
                        .Select(r => (decimal?)r.Score)
                        .Average() ?? 0,
                    RatingCount = _context.Ratings.Count(r => r.ItemId == i.Id)
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound("Item blev ikke fundet.");
            }

            return Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddItem([FromBody] ItemModel item)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Manglende bruger-id i token.");
            }

            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            item.UserId = userId;

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new ItemResponseDto
            {
                Id = item.Id,
                UserId = item.UserId,
                CreatedByUsername = User.Identity?.Name ?? "Ukendt bruger",
                Name = item.Name,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                AverageRating = 0,
                RatingCount = 0
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] ItemModel updatedItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Manglende bruger-id i token.");
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound("Item blev ikke fundet.");
            }

            if (!string.Equals(item.UserId, userId, StringComparison.Ordinal))
            {
                return Forbid();
            }

            item.Name = updatedItem.Name.Trim();
            item.Description = updatedItem.Description.Trim();
            item.ImageUrl = updatedItem.ImageUrl?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Manglende bruger-id i token.");
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound("Item blev ikke fundet.");
            }

            if (!string.Equals(item.UserId, userId, StringComparison.Ordinal))
            {
                return Forbid();
            }

            var ratings = await _context.Ratings
                .Where(r => r.ItemId == item.Id)
                .ToListAsync();

            _context.Ratings.RemoveRange(ratings);
            _context.Items.Remove(item);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}