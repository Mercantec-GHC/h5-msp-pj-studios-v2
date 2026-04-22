using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        // Simpel midlertidig liste for at gemme i hukommelsen (skiftes ud med database senere)
        private static readonly List<RatingsModel> _ratings = new();

        [HttpPost]
        public IActionResult AddRating([FromBody] RatingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Giver en midlertidig ID indtil database er klar
            model.Id = _ratings.Count > 0 ? _ratings.Max(r => r.Id) + 1 : 1;

            _ratings.Add(model);

            return Ok(model);
        }

        [HttpGet("item/{itemId}")]
        public IActionResult GetRatingsForItem(string itemId)
        {
            var itemRatings = _ratings.Where(r => r.ItemId == itemId).ToList();
            if (!itemRatings.Any())
            {
                return NotFound("Ingen ratings fundet for dette item.");
            }

            return Ok(itemRatings);
        }
    }
}
