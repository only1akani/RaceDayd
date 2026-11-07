using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    public class EventsController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {
            _api = api;
        }

        //the full details of one event, with its categories
        public async Task<IActionResult> Details(int id)
        {//start of method

            ApiResponse response = await _api.GetAsync($"api/events/{id}");

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.StatusCode == 404 ? "We could not find that event." : response.Message;
                return RedirectToAction("Index", "Home");
            }

            EventDetails? ev = response.Data<EventDetails>();
            if (ev == null)
            {
                TempData["Error"] = "We could not load that event. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            EventDetailsViewModel model = new EventDetailsViewModel { Event = ev };

            //participants can see which categories they have already entered
            if (HttpContext.Session.GetString(SessionKeys.Role) == "Participant")
            {
                ApiResponse mine = await _api.GetAsync("api/enrolments/my");

                IActionResult? expired = SessionExpired(mine);
                if (expired != null)
                {
                    return expired;
                }

                List<MyEnrolment> enrolments = mine.Data<List<MyEnrolment>>() ?? new List<MyEnrolment>();
                foreach (MyEnrolment enrolment in enrolments)
                {
                    model.MyEnrolments[enrolment.CategoryId] = enrolment.Status;
                }
            }

            return View(model);

        }//end of method

    }//end of class
}//end of namespace