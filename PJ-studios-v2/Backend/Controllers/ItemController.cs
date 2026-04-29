using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> AddItem([FromBody] ItemModel item)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }
    }
}
