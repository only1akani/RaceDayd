using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaceDay.Web.Services;

namespace RaceDay.Web.Filters
{//start of namespace

    //protects a page in the website
    //[RoleAuthorize] = any logged in user
    //[RoleAuthorize("Organiser")] = only organisers
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {//start of class

        private readonly string? _role;

        public RoleAuthorizeAttribute(string? role = null)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {//start of method

            int? userId = context.HttpContext.Session.GetInt32(SessionKeys.UserId);
            string? role = context.HttpContext.Session.GetString(SessionKeys.Role);

            ITempDataDictionary tempData = context.HttpContext.RequestServices
                .GetRequiredService<ITempDataDictionaryFactory>()
                .GetTempData(context.HttpContext);

            //not logged in, so send them to the login page
            if (userId == null)
            {
                tempData["Error"] = "Please log in to continue.";
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            //logged in but the wrong role, so send them home
            if (_role != null && role != _role)
            {
                tempData["Error"] = "You do not have access to that page.";
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

        }//end of method

    }//end of class
}//end of namespace