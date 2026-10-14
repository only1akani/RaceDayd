using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {//start of class

        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lists every event so participants can browse what is coming up. Public.
        /// </summary>
        /// <returns>200 with an array of events</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents()
        {//start of method

            var events = await _context.Events
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    eventId = e.EventId,
                    eventName = e.EventName,
                    eventDescription = e.EventDescription,
                    eventDate = e.EventDate,
                    location = e.Location,
                    organiserId = e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);

        }//end of method

        /// <summary>
        /// Gets the full details of one event, including its categories. Public.
        /// </summary>
        /// <param name="id">the event id</param>
        /// <returns>200 with the event, 404 if the event does not exist</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {//start of method

            var ev = await _context.Events
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    eventId = e.EventId,
                    eventName = e.EventName,
                    eventDescription = e.EventDescription,
                    eventDate = e.EventDate,
                    location = e.Location,
                    organiserId = e.OrganiserId,
                    categories = e.Categories.Select(c => new
                    {
                        categoryId = c.CategoryId,
                        categoryName = c.CategoryName,
                        distance = c.Distance,
                        price = c.Price,
                        maxParticipants = c.MaxParticipants
                    })
                })
                .FirstOrDefaultAsync();

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(ev);

        }//end of method

        /// <summary>
        /// Creates a new event. The logged in organiser becomes the owner.
        /// </summary>
        /// <param name="dto">eventName, eventDescription, eventDate and location</param>
        /// <returns>201 with the new eventId, 400 for missing fields, 401 if not logged in, 403 if not an Organiser</returns>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {//start of method

            Event ev = new Event
            {
                OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value,
                EventName = dto.EventName.Trim(),
                EventDescription = dto.EventDescription,
                EventDate = dto.EventDate!.Value,
                Location = dto.Location
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { eventId = ev.EventId });

        }//end of method

        /// <summary>
        /// Edits an existing event. Only the organiser who owns the event can edit it.
        /// </summary>
        /// <param name="id">the event id</param>
        /// <param name="dto">eventName, eventDescription, eventDate and location</param>
        /// <returns>200 when updated, 400 for missing fields, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist</returns>
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, EventDto dto)
        {//start of method

            Event? ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            //only the owner can edit
            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            ev.EventName = dto.EventName.Trim();
            ev.EventDescription = dto.EventDescription;
            ev.EventDate = dto.EventDate!.Value;
            ev.Location = dto.Location;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated." });

        }//end of method

        /// <summary>
        /// Deletes an event and everything under it. Only the organiser who owns the event can delete it.
        /// </summary>
        /// <param name="id">the event id</param>
        /// <returns>200 when deleted, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist</returns>
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {//start of method

            Event? ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted." });

        }//end of method

    }//end of class
}//end of namespace