using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{//start of namespace

    //checks the session before an endpoint runs
    //[SessionAuthorize] = any logged in user
    //[SessionAuthorize("Organiser")] = only organisers
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {//start of class

        private readonly string? _role;

        public SessionAuthorizeAttribute(string? role = null)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {//start of method

            int? userId = context.HttpContext.Session.GetInt32("UserId");
            string? role = context.HttpContext.Session.GetString("Role");

            //not logged in = 401
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
                return;
            }

            //logged in but wrong role = 403
            if (_role != null && role != _role)
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to do this." })
                {
                    StatusCode = 403
                };
            }

        }//end of method

    }//end of class
}//end of namespace