using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {//start of class

        //the event types an organiser can choose from
        private static readonly string[] EventTypes = { "Run", "Walk", "Cycle" };

        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;

        public EventsController(RaceDayDbContext context, IBlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        //returns the correctly spelled event type, or null if it is not Run, Walk or Cycle
        private static string? MatchEventType(string value)
        {
            return EventTypes.FirstOrDefault(t => t.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Lists events so participants can browse what is coming up. Public. Can be filtered by type, search text and upcoming only.
        /// </summary>
        /// <param name="type">optional: Run, Walk or Cycle</param>
        /// <param name="search">optional: text to look for in the event name or location</param>
        /// <param name="upcoming">optional: true to only show events that have not happened yet</param>
        /// <returns>200 with an array of events</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents(string? type, string? search, bool upcoming = false)
        {//start of method

            IQueryable<Event> query = _context.Events;

            if (!string.IsNullOrWhiteSpace(type))
            {
                string eventType = type.Trim();
                query = query.Where(e => e.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim().ToLower();
                query = query.Where(e => e.EventName.ToLower().Contains(text)
                    || (e.Location != null && e.Location.ToLower().Contains(text)));
            }

            if (upcoming)
            {
                DateTime now = DateTime.Now;
                query = query.Where(e => e.EventDate >= now);
            }

            var events = await query
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    eventId = e.EventId,
                    eventName = e.EventName,
                    eventDescription = e.EventDescription,
                    eventDate = e.EventDate,
                    location = e.Location,
                    eventType = e.EventType,
                    distance = e.Distance,
                    bannerUrl = e.BannerUrl,
                    organiserId = e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);

        }//end of method

        /// <summary>
        /// Lists the events owned by the logged in organiser, with enrolment counts for the dashboard.
        /// </summary>
        /// <returns>200 with an array of the organiser's events, 401 if not logged in, 403 if not an Organiser</returns>
        [HttpGet("mine")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEvents()
        {//start of method

            int organiserId = HttpContext.Session.GetInt32("UserId")!.Value;

            var events = await _context.Events
                .Where(e => e.OrganiserId == organiserId)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    eventId = e.EventId,
                    eventName = e.EventName,
                    eventDescription = e.EventDescription,
                    eventDate = e.EventDate,
                    location = e.Location,
                    eventType = e.EventType,
                    distance = e.Distance,
                    bannerUrl = e.BannerUrl,
                    categoryCount = e.Categories.Count,
                    enrolmentCount = _context.Enrolments.Count(en => en.Category!.EventId == e.EventId),
                    confirmedCount = _context.Enrolments.Count(en => en.Category!.EventId == e.EventId && en.Status == "Confirmed"),
                    resultCount = _context.Results.Count(r => r.Enrolment!.Category!.EventId == e.EventId)
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
                    eventType = e.EventType,
                    distance = e.Distance,
                    bannerUrl = e.BannerUrl,
                    organiserId = e.OrganiserId,
                    organiserName = e.Organiser!.FullName,
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
        /// <param name="dto">eventName, eventDescription, eventDate, location, eventType (Run, Walk or Cycle) and distance</param>
        /// <returns>201 with the new eventId, 400 for missing fields or an invalid event type, 401 if not logged in, 403 if not an Organiser</returns>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {//start of method

            string eventType = "Run";
            if (!string.IsNullOrWhiteSpace(dto.EventType))
            {
                string? matched = MatchEventType(dto.EventType);
                if (matched == null)
                {
                    return BadRequest(new { message = "Event type must be Run, Walk or Cycle." });
                }
                eventType = matched;
            }

            Event ev = new Event
            {
                OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value,
                EventName = dto.EventName.Trim(),
                EventDescription = dto.EventDescription,
                EventDate = dto.EventDate!.Value,
                Location = dto.Location,
                EventType = eventType,
                Distance = dto.Distance ?? 0
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { eventId = ev.EventId });

        }//end of method

        /// <summary>
        /// Edits an existing event. Only the organiser who owns the event can edit it.
        /// </summary>
        /// <param name="id">the event id</param>
        /// <param name="dto">eventName, eventDescription, eventDate, location, eventType and distance</param>
        /// <returns>200 when updated, 400 for missing fields or an invalid event type, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist</returns>
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

            //keep the old type if none was sent
            if (!string.IsNullOrWhiteSpace(dto.EventType))
            {
                string? matched = MatchEventType(dto.EventType);
                if (matched == null)
                {
                    return BadRequest(new { message = "Event type must be Run, Walk or Cycle." });
                }
                ev.EventType = matched;
            }

            ev.EventName = dto.EventName.Trim();
            ev.EventDescription = dto.EventDescription;
            ev.EventDate = dto.EventDate!.Value;
            ev.Location = dto.Location;

            //keep the old distance if none was sent
            if (dto.Distance != null)
            {
                ev.Distance = dto.Distance.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated." });

        }//end of method

        /// <summary>
        /// Uploads a banner image for an event to Azure Blob Storage and saves the link. Only the owner of the event can upload one.
        /// </summary>
        /// <param name="id">the event id</param>
        /// <param name="file">an image file (.jpg, .jpeg, .png or .webp, up to 5 MB)</param>
        /// <returns>200 with the bannerUrl, 400 for an invalid file, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist, 500 if the upload to Azure fails</returns>
        [HttpPost("{id}/banner")]
        [SessionAuthorize("Organiser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
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

            string? problem = ImageRules.Validate(file);
            if (problem != null)
            {
                return BadRequest(new { message = problem });
            }

            try
            {
                ev.BannerUrl = await _blobService.UploadImageAsync(BlobContainers.EventBanners, file);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "The image could not be uploaded. Please try again." });
            }

            return Ok(new { bannerUrl = ev.BannerUrl });

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