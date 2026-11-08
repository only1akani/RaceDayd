using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //the profile page for both roles
    [RoleAuthorize]
    public class ProfileController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ProfileController(ApiClient api)
        {
            _api = api;
        }

        private int CurrentUserId()
        {
            return HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        }

        private static ProfileViewModel ToModel(UserProfile profile)
        {
            return new ProfileViewModel
            {
                FullName = profile.FullName,
                Email = profile.Email,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                Role = profile.Role
            };
        }

        public async Task<IActionResult> Index()
        {//start of method

            ApiResponse response = await _api.GetAsync($"api/users/{CurrentUserId()}");

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            UserProfile? profile = response.Data<UserProfile>();
            if (!response.IsSuccess || profile == null)
            {
                TempData["Error"] = response.IsSuccess ? "We could not load your profile." : response.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(ToModel(profile));

        }//end of method

        //saves the new name and email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {//start of method

            //the picture and role are not part of the form, so load them again for the page
            ApiResponse current = await _api.GetAsync($"api/users/{CurrentUserId()}");

            IActionResult? expired = SessionExpired(current);
            if (expired != null)
            {
                return expired;
            }

            UserProfile? profile = current.Data<UserProfile>();
            model.ProfilePictureUrl = profile?.ProfilePictureUrl;
            model.Role = profile?.Role ?? string.Empty;

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            ApiResponse response = await _api.PutAsync($"api/users/{CurrentUserId()}", new
            {
                fullName = model.FullName,
                email = model.Email
            });

            expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                //keep the name in the menu up to date
                HttpContext.Session.SetString(SessionKeys.FullName, model.FullName.Trim());
                TempData["Success"] = "Your profile has been updated.";
                return RedirectToAction("Index");
            }

            if (response.StatusCode == 409)
            {
                ModelState.AddModelError(nameof(model.Email), response.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }

            return View("Index", model);

        }//end of method

        //sends the new profile picture to the API, which stores it in Azure Blob Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {//start of method

            if (picture == null || picture.Length == 0)
            {
                TempData["Error"] = "Please choose a picture first.";
                return RedirectToAction("Index");
            }

            ApiResponse response = await _api.UploadAsync($"api/users/{CurrentUserId()}/profile-picture", picture);

            IActionResult? expired = SessionExpired(response);
            if (expired != null)
            {
                return expired;
            }

            if (response.IsSuccess)
            {
                TempData["Success"] = "Your profile picture has been updated.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Index");

        }//end of method

    }//end of class
}//end of namespace