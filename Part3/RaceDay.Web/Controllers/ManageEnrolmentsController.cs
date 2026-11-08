using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //see who has entered an event and confirm their enrolments
    [RoleAuthorize("Organiser")]
    public class ManageEnrolmentsController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ManageEnrolmentsController(ApiClient api)
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

            EnrolmentsPageViewModel model = new EnrolmentsPageViewModel { Event = ev! };

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

        //confirms an enrolment, or puts it back to pending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int eventId, int enrolmentId, string status)
        {//start of method

            ApiResponse response = await _api.PutAsync($"api/enrolments/{enrolmentId}/status", new { status });

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = status == "Confirmed" ? "The enrolment has been confirmed." : "The enrolment is pending again.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Index", new { eventId });

        }//end of method

    }//end of class
}//end of namespace