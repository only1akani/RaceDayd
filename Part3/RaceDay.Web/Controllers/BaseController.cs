using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //shared behaviour for the pages that need a logged in user
    public abstract class BaseController : Controller
    {//start of class

        //if the API says we are not logged in (for example it was restarted), send the user to the login page
        protected IActionResult? SessionExpired(ApiResponse response)
        {//start of method

            if (!response.IsUnauthorized)
            {
                return null;
            }

            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has ended. Please log in again.";
            return RedirectToAction("Login", "Account");

        }//end of method

        //loads an event and checks that the logged in organiser owns it
        protected async Task<(EventDetails? Event, IActionResult? Problem)> LoadOwnEvent(ApiClient api, int eventId)
        {//start of method

            ApiResponse response = await api.GetAsync($"api/events/{eventId}");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return (null, expired);
            }

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.StatusCode == 404 ? "We could not find that event." : response.Message;
                return (null, RedirectToAction("Index", "ManageEvents"));
            }

            EventDetails? ev = response.Data<EventDetails>();
            if (ev == null)
            {
                TempData["Error"] = "We could not load that event. Please try again.";
                return (null, RedirectToAction("Index", "ManageEvents"));
            }

            //organisers can only manage their own events
            if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            {
                TempData["Error"] = "You can only manage your own events.";
                return (null, RedirectToAction("Index", "ManageEvents"));
            }

            return (ev, null);

        }//end of method

    }//end of class
}//end of namespace