using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //capture the finish times and positions after an event
    [RoleAuthorize("Organiser")]
    public class ManageResultsController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ManageResultsController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int eventId)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, eventId);
            if (problem != null)
            {
                return problem;
            }

            ApiResponse response = await _api.GetAsync($"api/events/{eventId}/enrolments");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            ResultsPageViewModel model = new ResultsPageViewModel { Event = ev! };

            if (response.IsSuccess)
            {
                model.Enrolments = response.Data<List<EventEnrolment>>() ?? new List<EventEnrolment>();
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return View(model);

        }//end of method

        //saves the result of one participant; it is public as soon as it is saved
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int eventId, int enrolmentId, string? finishTime, int? position)
        {//start of method

            string time = (finishTime ?? string.Empty).Trim();

            if (!Regex.IsMatch(time, @"^\d{2}:[0-5]\d:[0-5]\d$"))
            {
                TempData["Error"] = "The finish time must look like 00:48:32 (hours:minutes:seconds).";
                return RedirectToAction("Index", new { eventId });
            }

            if (position == null || position < 1)
            {
                TempData["Error"] = "Please enter a finishing position of 1 or more.";
                return RedirectToAction("Index", new { eventId });
            }

            ApiResponse response = await _api.PostAsync($"api/enrolments/{enrolmentId}/results", new { finishTime = time, position });

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "The result has been saved and published.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Index", new { eventId });

        }//end of method

    }//end of class
}//end of namespace