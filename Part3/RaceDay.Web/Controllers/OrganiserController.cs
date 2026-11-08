using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //the organiser's home page
    [RoleAuthorize("Organiser")]
    public class OrganiserController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Dashboard()
        {//start of method

            ApiResponse response = await _api.GetAsync("api/events/mine");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            DashboardViewModel model = new DashboardViewModel();
            if (response.IsSuccess)
            {
                model.Events = response.Data<List<OrganiserEvent>>() ?? new List<OrganiserEvent>();
            }
            else
            {
                model.ErrorMessage = response.Message;
            }

            return View(model);

        }//end of method

    }//end of class
}//end of namespace