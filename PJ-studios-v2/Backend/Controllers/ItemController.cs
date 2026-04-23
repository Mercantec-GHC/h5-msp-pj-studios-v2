using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        // Simpel midlertidig liste for at gemme i hukommelsen (skiftes ud med database senere)
        private static readonly List<ItemModel> _items = new();

        [HttpGet]
        public IActionResult GetAllItems()
        {
            return Ok(_items);
        }

        [HttpGet("{id}")]
        public IActionResult GetItem(string id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return NotFound("Item blev ikke fundet.");
            }
            return Ok(item);
        }

        [HttpPost]
        public IActionResult AddItem([FromBody] ItemModel item)
        {
            // Opret et unikt ID, hvis det ikke allerede er sat
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            _items.Add(item);

            return Ok(item);
        }
    }
}
