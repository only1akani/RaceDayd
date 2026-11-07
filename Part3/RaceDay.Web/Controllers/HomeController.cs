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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //temporary check that the website can reach the API (removed later)
        public async Task<IActionResult> Ping()
        {//start of method

            ApiResponse response = await _api.GetAsync("api/events");

            if (response.IsSuccess)
            {
                return Content($"Connected to the API. Status {response.StatusCode}.");
            }

            return Content(response.Message);

        }//end of method

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }//end of class
}//end of namespace