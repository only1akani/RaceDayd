using Microsoft.AspNetCore.Mvc;
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

    }//end of class
}//end of namespace