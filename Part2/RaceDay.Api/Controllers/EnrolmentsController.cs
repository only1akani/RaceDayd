using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    public class EnrolmentsController : ControllerBase
    {//start of class

        //the statuses an enrolment can have
        private static readonly string[] Statuses = { "Pending", "Confirmed" };

        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Enrols the logged in participant into a category. The status is set to Pending.
        /// </summary>
        /// <param name="categoryId">the category id</param>
        /// <returns>201 with the enrolment record, 401 if not logged in, 403 if not a Participant, 404 if the category does not exist, 409 if already enrolled in this category or the category is full</returns>
        [HttpPost("api/categories/{categoryId}/enrolments")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int categoryId)
        {//start of method

            Category? category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            //the participant comes from the session, not the request
            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            //one enrolment per participant per category
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == participantId && e.CategoryId == categoryId);
            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            //the category must still have space
            int taken = await _context.Enrolments.CountAsync(e => e.CategoryId == categoryId);
            if (taken >= category.MaxParticipants)
            {
                return Conflict(new { message = "This category is full." });
            }

            Enrolment enrolment = new Enrolment
            {
                ParticipantId = participantId,
                CategoryId = categoryId,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                enrolmentId = enrolment.EnrolmentId,
                categoryId = enrolment.CategoryId,
                status = enrolment.Status,
                enrolmentDate = enrolment.EnrolmentDate
            });

        }//end of method

        /// <summary>
        /// Gets every enrolment that belongs to the logged in participant, with the event details.
        /// </summary>
        /// <returns>200 with an array of the participant's enrolments, 401 if not logged in, 403 if not a Participant</returns>
        [HttpGet("api/enrolments/my")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEnrolments()
        {//start of method

            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == participantId)
                .OrderBy(e => e.Category!.Event!.EventDate)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentId,
                    status = e.Status,
                    enrolmentDate = e.EnrolmentDate,
                    categoryId = e.CategoryId,
                    categoryName = e.Category!.CategoryName,
                    price = e.Category.Price,
                    eventId = e.Category.EventId,
                    eventName = e.Category.Event!.EventName,
                    eventDate = e.Category.Event.EventDate,
                    location = e.Category.Event.Location,
                    eventType = e.Category.Event.EventType,
                    bannerUrl = e.Category.Event.BannerUrl
                })
                .ToListAsync();

            return Ok(enrolments);

        }//end of method

        /// <summary>
        /// Gets every enrolment for one event so the organiser can see who has entered, including any captured result. Only the owner of the event can view them.
        /// </summary>
        /// <param name="eventId">the event id</param>
        /// <returns>200 with an array of enrolments, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the event does not exist</returns>
        [HttpGet("api/events/{eventId}/enrolments")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {//start of method

            Event? ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            //only the owner can see who entered
            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            var enrolments = await _context.Enrolments
                .Where(e => e.Category!.EventId == eventId)
                .OrderBy(e => e.Participant!.FullName)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentId,
                    status = e.Status,
                    enrolmentDate = e.EnrolmentDate,
                    participantId = e.ParticipantId,
                    participantName = e.Participant!.FullName,
                    participantEmail = e.Participant.Email,
                    categoryId = e.CategoryId,
                    categoryName = e.Category!.CategoryName,
                    resultId = e.Result != null ? (int?)e.Result.ResultId : null,
                    finishTime = e.Result != null ? e.Result.FinishTime : null,
                    position = e.Result != null ? e.Result.Position : null
                })
                .ToListAsync();

            return Ok(enrolments);

        }//end of method

        /// <summary>
        /// Changes the status of an enrolment to Pending or Confirmed. Only the organiser who owns the event can do this.
        /// </summary>
        /// <param name="id">the enrolment id</param>
        /// <param name="dto">status (Pending or Confirmed)</param>
        /// <returns>200 when updated, 400 for an invalid status, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the enrolment does not exist</returns>
        [HttpPut("api/enrolments/{id}/status")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, EnrolmentStatusDto dto)
        {//start of method

            Enrolment? enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            //only the organiser of the event can change the status
            if (enrolment.Category!.Event!.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            string? status = Statuses.FirstOrDefault(s => s.Equals(dto.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            if (status == null)
            {
                return BadRequest(new { message = "Status must be Pending or Confirmed." });
            }

            enrolment.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment updated.", status = enrolment.Status });

        }//end of method

        /// <summary>
        /// Cancels an enrolment. A participant can only cancel their own enrolment.
        /// </summary>
        /// <param name="id">the enrolment id</param>
        /// <returns>200 when cancelled, 401 if not logged in, 403 if it is not your enrolment or you are not a Participant, 404 if the enrolment does not exist</returns>
        [HttpDelete("api/enrolments/{id}")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelEnrolment(int id)
        {//start of method

            Enrolment? enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (enrolment.ParticipantId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "This is not your enrolment." });
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled." });

        }//end of method

    }//end of class
}//end of namespace