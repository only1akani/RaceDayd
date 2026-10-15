using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    public class ResultsController : ControllerBase
    {//start of class

        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Captures the finish time and position for one enrolment after the race. Only the organiser who owns the event can capture it.
        /// </summary>
        /// <param name="enrolmentId">the enrolment id</param>
        /// <param name="dto">finishTime (hh:mm:ss) and position</param>
        /// <returns>201 when saved, 400 for an invalid time or position, 401 if not logged in, 403 if not the owner or not an Organiser, 404 if the enrolment does not exist, 409 if a result was already captured</returns>
        [HttpPost("api/enrolments/{enrolmentId}/results")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult(int enrolmentId, ResultDto dto)
        {//start of method

            Enrolment? enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            //only the organiser of the event can capture results
            if (enrolment.Category!.Event!.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You do not own this event." });
            }

            //the finish time must look like 00:48:32
            if (!TimeSpan.TryParseExact(dto.FinishTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out TimeSpan finishTime)
                || finishTime <= TimeSpan.Zero)
            {
                return BadRequest(new { message = "Finish time must be in the format hh:mm:ss, for example 00:48:32." });
            }

            //only one result per enrolment
            bool alreadyCaptured = await _context.Results.AnyAsync(r => r.EnrolmentId == enrolmentId);
            if (alreadyCaptured)
            {
                return Conflict(new { message = "A result was already captured for this enrolment." });
            }

            Result result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = finishTime,
                Position = dto.Position!.Value
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                resultId = result.ResultId,
                enrolmentId = result.EnrolmentId,
                finishTime = finishTime.ToString(@"hh\:mm\:ss"),
                position = result.Position
            });

        }//end of method

        /// <summary>
        /// Gets all the results that belong to the logged in participant, so they can track their performance history.
        /// </summary>
        /// <returns>200 with an array of the participant's results, 401 if not logged in, 403 if not a Participant</returns>
        [HttpGet("api/results/my")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {//start of method

            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            var results = await _context.Results
                .Where(r => r.Enrolment!.ParticipantId == participantId)
                .Select(r => new
                {
                    resultId = r.ResultId,
                    enrolmentId = r.EnrolmentId,
                    eventName = r.Enrolment!.Category!.Event!.EventName,
                    eventDate = r.Enrolment.Category.Event.EventDate,
                    categoryName = r.Enrolment.Category.CategoryName,
                    finishTime = r.FinishTime,
                    position = r.Position,
                    recordedAt = r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);

        }//end of method

        /// <summary>
        /// Gets all the results for an event, for the organiser to review or for a public results list. Public.
        /// </summary>
        /// <param name="eventId">the event id</param>
        /// <returns>200 with an array of results ordered by position, 404 if the event does not exist</returns>
        [HttpGet("api/events/{eventId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventResults(int eventId)
        {//start of method

            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var results = await _context.Results
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    resultId = r.ResultId,
                    participantName = r.Enrolment!.Participant!.FullName,
                    categoryName = r.Enrolment.Category!.CategoryName,
                    finishTime = r.FinishTime,
                    position = r.Position
                })
                .ToListAsync();

            return Ok(results);

        }//end of method

    }//end of class
}//end of namespace