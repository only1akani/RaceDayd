using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //add, edit and delete the categories of an event
    [RoleAuthorize("Organiser")]
    public class ManageCategoriesController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ManageCategoriesController(ApiClient api)
        {
            _api = api;
        }

        private static object ToPayload(CategoryFormViewModel form)
        {
            return new
            {
                categoryName = form.CategoryName,
                distance = form.Distance,
                price = form.Price,
                maxParticipants = form.MaxParticipants
            };
        }

        public async Task<IActionResult> Index(int eventId)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, eventId);
            if (problem != null)
            {
                return problem;
            }

            return View(new CategoriesPageViewModel
            {
                Event = ev!,
                Form = new CategoryFormViewModel { EventId = eventId }
            });

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Form")] CategoryFormViewModel form)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, form.EventId);
            if (problem != null)
            {
                return problem;
            }

            if (!ModelState.IsValid)
            {
                return View("Index", new CategoriesPageViewModel { Event = ev!, Form = form });
            }

            ApiResponse response = await _api.PostAsync($"api/events/{form.EventId}/categories", ToPayload(form));

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "The category has been added.";
                return RedirectToAction("Index", new { eventId = form.EventId });
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return View("Index", new CategoriesPageViewModel { Event = ev!, Form = form });

        }//end of method

        [HttpGet]
        public async Task<IActionResult> Edit(int eventId, int id)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, eventId);
            if (problem != null)
            {
                return problem;
            }

            CategoryInfo? category = ev!.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                TempData["Error"] = "We could not find that category.";
                return RedirectToAction("Index", new { eventId });
            }

            return View(new CategoriesPageViewModel
            {
                Event = ev,
                Form = new CategoryFormViewModel
                {
                    EventId = eventId,
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Distance = category.Distance,
                    Price = category.Price,
                    MaxParticipants = category.MaxParticipants
                }
            });

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind(Prefix = "Form")] CategoryFormViewModel form)
        {//start of method

            var (ev, problem) = await LoadOwnEvent(_api, form.EventId);
            if (problem != null)
            {
                return problem;
            }

            if (!ModelState.IsValid)
            {
                return View(new CategoriesPageViewModel { Event = ev!, Form = form });
            }

            ApiResponse response = await _api.PutAsync($"api/categories/{form.CategoryId}", ToPayload(form));

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "The category has been updated.";
                return RedirectToAction("Index", new { eventId = form.EventId });
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return View(new CategoriesPageViewModel { Event = ev!, Form = form });

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eventId, int id)
        {//start of method

            ApiResponse response = await _api.DeleteAsync($"api/categories/{id}");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "The category has been deleted.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Index", new { eventId });

        }//end of method

    }//end of class
}//end of namespace