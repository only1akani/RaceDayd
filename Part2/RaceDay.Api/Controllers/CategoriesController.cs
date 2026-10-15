using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    public class CategoriesController : ControllerBase
    {//start of class

        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lists all the categories (like 5km, 10km, 21km) that belong to one event. Public.
        /// </summary>
        /// <param name="eventId">the event id</param>
        /// <returns>200 with an array of categories, 404 if the event does not exist</returns>
        [HttpGet("api/events/{eventId}/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories(int eventId)
        {//start of method

            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    categoryId = c.CategoryId,
                    categoryName = c.CategoryName,
                    distance = c.Distance,
                    price = c.Price,
                    maxParticipants = c.MaxParticipants
                })
                .ToListAsync();

            return Ok(categories);

        }//end of method

        /// <summary>
        /// Adds a new category to an event. Only the organiser who owns the event can add one.
        /// </summary>
        /// <param name="eventId">the event id</param>
        /// <param name="dto">categoryName, distance, price and maxParticipants</param>
        /// <returns>201 with the new categoryId, 400 for invalid data, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist</returns>
        [HttpPost("api/events/{eventId}/categories")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryDto dto)
        {//start of method

            Event? ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            //only the owner of the event can add categories
            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            Category category = new Category
            {
                EventId = eventId,
                CategoryName = dto.CategoryName.Trim(),
                Distance = dto.Distance!.Value,
                Price = dto.Price ?? 0,
                MaxParticipants = dto.MaxParticipants ?? 100
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { categoryId = category.CategoryId });

        }//end of method

        /// <summary>
        /// Edits a category, for example changing the price or the max participants. Only the owner of the event can edit it.
        /// </summary>
        /// <param name="id">the category id</param>
        /// <param name="dto">categoryName, distance, price and maxParticipants</param>
        /// <returns>200 when updated, 400 for invalid data, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the category does not exist</returns>
        [HttpPut("api/categories/{id}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {//start of method

            Category? category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.Event!.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            category.CategoryName = dto.CategoryName.Trim();
            category.Distance = dto.Distance!.Value;
            category.Price = dto.Price ?? 0;
            category.MaxParticipants = dto.MaxParticipants ?? 100;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated." });

        }//end of method

        /// <summary>
        /// Removes a category from an event. Only the owner of the event can delete it.
        /// </summary>
        /// <param name="id">the category id</param>
        /// <returns>200 when deleted, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the category does not exist</returns>
        [HttpDelete("api/categories/{id}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {//start of method

            Category? category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.Event!.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted." });

        }//end of method

    }//end of class
}//end of namespace