using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    public class HomeController : Controller
    {//start of class

        private readonly ApiClient _api;

        public HomeController(ApiClient api)
        {
            _api = api;
        }

        //the events page: upcoming events with a search and a type filter
        public async Task<IActionResult> Index(string? type, string? search)
        {//start of method

            HomeViewModel model = new HomeViewModel { Type = type, Search = search };

            string url = "api/events?upcoming=true";
            if (!string.IsNullOrWhiteSpace(type))
            {
                url += "&type=" + Uri.EscapeDataString(type);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            ApiResponse response = await _api.GetAsync(url);
            if (response.IsSuccess)
            {
                model.Events = response.Data<List<EventSummary>>() ?? new List<EventSummary>();
            }
            else
            {
                model.ErrorMessage = response.Message;
            }

            return View(model);

        }//end of method

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }//end of class
}//end of namespace