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
            var items = await _context.Items.ToListAsync();
            return Ok(items);
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _context.Items
                .SelectMany(i => i.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(string id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
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

            return Ok(item);
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
            item.Tags = updatedItem.Tags ?? new List<string>();

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

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
