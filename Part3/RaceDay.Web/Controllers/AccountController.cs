using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    public class AccountController : Controller
    {//start of class

        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        //what the API sends back after a login
        private class LoginResult
        {
            public int UserId { get; set; }
            public string Role { get; set; } = string.Empty;
        }

        //the part of the profile we need for the menu
        private class ProfileResult
        {
            public string FullName { get; set; } = string.Empty;
        }

        [HttpGet]
        public IActionResult Register()
        {
            //people who are already logged in do not need this page
            if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {//start of method

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApiResponse response = await _api.PostAsync("api/auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                idNumber = model.IdNumber,
                role = model.Role
            });

            if (response.IsSuccess)
            {
                TempData["Success"] = "Your account has been created. Please log in.";
                return RedirectToAction("Login");
            }

            //show the problem next to the email field when the email is taken
            if (response.StatusCode == 409)
            {
                ModelState.AddModelError(nameof(model.Email), response.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }

            return View(model);

        }//end of method

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {//start of method

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //start from a clean session
            HttpContext.Session.Clear();

            ApiResponse login = await _api.PostAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!login.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, login.Message);
                return View(model);
            }

            LoginResult? result = login.Data<LoginResult>();
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                return View(model);
            }

            //remember who is logged in and which role they have
            HttpContext.Session.SetInt32(SessionKeys.UserId, result.UserId);
            HttpContext.Session.SetString(SessionKeys.Role, result.Role);

            //get the name for the menu (the API session cookie is sent automatically)
            ApiResponse profile = await _api.GetAsync($"api/users/{result.UserId}");
            ProfileResult? details = profile.Data<ProfileResult>();
            string name = string.IsNullOrWhiteSpace(details?.FullName) ? model.Email : details!.FullName;
            HttpContext.Session.SetString(SessionKeys.FullName, name);

            TempData["Success"] = $"Welcome back, {name}!";

            //organisers will go to their dashboard once it exists (Step 6)
            return RedirectToAction("Index", "Home");

        }//end of method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {//start of method

            //end the session on the API as well, then clear ours
            await _api.PostAsync("api/auth/logout");
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Login");

        }//end of method

    }//end of class
}//end of namespace