using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{//start of namespace

    //the participant's race history
    [RoleAuthorize("Participant")]
    public class ResultsController : BaseController
    {//start of class

        private readonly ApiClient _api;

        public ResultsController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> MyResults()
        {//start of method

            MyResultsViewModel model = new MyResultsViewModel();

            ApiResponse resultsResponse = await _api.GetAsync("api/results/my");

            IActionResult? expired = SessionExpired(resultsResponse);
            if (expired != null)
            {
                return expired;
            }

            if (!resultsResponse.IsSuccess)
            {
                TempData["Error"] = resultsResponse.Message;
                return View(model);
            }

            model.Results = resultsResponse.Data<List<MyResult>>() ?? new List<MyResult>();

            //events that have finished but have no result captured yet
            ApiResponse enrolmentsResponse = await _api.GetAsync("api/enrolments/my");
            if (enrolmentsResponse.IsSuccess)
            {
                List<MyEnrolment> enrolments = enrolmentsResponse.Data<List<MyEnrolment>>() ?? new List<MyEnrolment>();
                HashSet<int> captured = model.Results.Select(r => r.EnrolmentId).ToHashSet();

                model.AwaitingResults = enrolments
                    .Where(e => e.EventDate < DateTime.Now && !captured.Contains(e.EnrolmentId))
                    .ToList();
            }

            return View(model);

        }//end of method

    }//end of class
}//end of namespace