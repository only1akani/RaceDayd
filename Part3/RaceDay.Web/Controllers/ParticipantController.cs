using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //pages that only participants can use
    [RoleAuthorize("Participant")]
    public class ParticipantController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        //every event the participant has entered, with the status of each enrolment
        public async Task<IActionResult> MyEvents()
        {//start of method

            ApiResponse response = await _api.GetAsync("api/enrolments/my");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Message;
                return View(new List<MyEnrolment>());
            }

            return View(response.Data<List<MyEnrolment>>() ?? new List<MyEnrolment>());

        }//end of method

        //enters the participant into the chosen category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {//start of method

            ApiResponse response = await _api.PostAsync($"api/categories/{categoryId}/enrolments");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "You are entered! Your enrolment is Pending until the organiser confirms it.";
                return RedirectToAction("MyEvents");
            }

            //for example "You are already enrolled in this category." or "This category is full."
            TempData["Error"] = response.Message;
            return RedirectToAction("Details", "Events", new { id = eventId });

        }//end of method

        //cancels one of the participant's own enrolments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {//start of method

            ApiResponse response = await _api.DeleteAsync($"api/enrolments/{id}");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "Your enrolment has been cancelled.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("MyEvents");

        }//end of method

    }//end of class
}//end of namespace