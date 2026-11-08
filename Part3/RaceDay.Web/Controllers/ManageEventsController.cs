using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //create, edit and delete events
    [RoleAuthorize("Organiser")]
    public class ManageEventsController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ManageEventsController(ApiClient api)
        {
            _api = api;
        }

        //what the API sends back after an event is created
        private class CreatedEvent
        {
            public int EventId { get; set; }
        }

        private static object ToPayload(EventFormViewModel model)
        {
            return new
            {
                eventName = model.EventName,
                eventDescription = model.EventDescription,
                eventDate = model.EventDate,
                location = model.Location,
                eventType = model.EventType,
                distance = model.Distance
            };
        }

        //sends the chosen banner to the API (which stores it in Azure) and returns a problem message, or null when fine
        private async Task<string?> UploadBannerAsync(int eventId, IFormFile? banner)
        {//start of method

            if (banner == null || banner.Length == 0)
            {
                return null;
            }

            ApiResponse response = await _api.UploadAsync($"api/events/{eventId}/banner", banner);
            return response.IsSuccess ? null : response.Message;

        }//end of method

        public async Task<IActionResult> Index()
        {//start of method

            ApiResponse response = await _api.GetAsync("api/events/mine");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Message;
                return View(new List<OrganiserEvent>());
            }

            return View(response.Data<List<OrganiserEvent>>() ?? new List<OrganiserEvent>());

        }//end of method

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EventFormViewModel { EventDate = DateTime.Today.AddDays(30).AddHours(6) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model, IFormFile? banner)
        {//start of method

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApiResponse response = await _api.PostAsync("api/events", ToPayload(model));

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(model);
            }

            int eventId = response.Data<CreatedEvent>()?.EventId ?? 0;
            if (eventId == 0)
            {
                TempData["Success"] = "Your event has been created.";
                return RedirectToAction("Index");
            }

            string? bannerProblem = await UploadBannerAsync(eventId, banner);
            if (bannerProblem != null)
            {
                TempData["Error"] = "Your event was created, but the banner could not be uploaded: " + bannerProblem;
                return RedirectToAction("Edit", new { id = eventId });
            }

            TempData["Success"] = "Your event has been created. Now add some categories so participants can enter.";
            return RedirectToAction("Index", "ManageCategories", new { eventId });

        }//end of method

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, id);
            if (problem != null)
            {
                return problem;
            }

            return View(new EventFormViewModel
            {
                EventId = ev!.EventId,
                EventName = ev.EventName,
                EventDescription = ev.EventDescription,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Distance = ev.Distance,
                BannerUrl = ev.BannerUrl
            });

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model, IFormFile? banner)
        {//start of method

            model.EventId = id;

            //load the saved banner so it can be shown again if the form has a problem
            var (ev, problem) = await LoadOwnEvent(_api, id);
            if (problem != null)
            {
                return problem;
            }
            model.BannerUrl = ev!.BannerUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApiResponse response = await _api.PutAsync($"api/events/{id}", ToPayload(model));

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(model);
            }

            string? bannerProblem = await UploadBannerAsync(id, banner);
            if (bannerProblem != null)
            {
                TempData["Error"] = "Your changes were saved, but the banner could not be uploaded: " + bannerProblem;
                return RedirectToAction("Edit", new { id });
            }

            TempData["Success"] = "Your event has been updated.";
            return RedirectToAction("Index");

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {//start of method

            ApiResponse response = await _api.DeleteAsync($"api/events/{id}");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "The event has been deleted.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Index");

        }//end of method

    }//end of class
}//end of namespace