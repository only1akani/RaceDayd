using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //tests that each role is rejected from the other role's endpoints
    public class RoleTests : ApiTestBase
    {//start of class

        [Fact]
        public async Task CreateEvent_NotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await NewClient().PostAsJsonAsync("/api/events", EventBody());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403()
        {
            var participant = await LoginAs("part@test.com", "Participant");

            HttpResponseMessage response = await participant.Client.PostAsJsonAsync("/api/events", EventBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_AsParticipant_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            var participant = await LoginAs("part@test.com", "Participant");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage response = await participant.Client.PostAsJsonAsync($"/api/events/{eventId}/categories",
                new { categoryName = "5km", distance = 5 });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);
            int categoryId = await CreateCategory(organiser.Client, eventId);

            HttpResponseMessage response = await organiser.Client.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task MyEnrolments_AsOrganiser_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.GetAsync("/api/enrolments/my");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task EventEnrolments_AsParticipant_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            var participant = await LoginAs("part@test.com", "Participant");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage response = await participant.Client.GetAsync($"/api/events/{eventId}/enrolments");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_AsParticipant_Returns403()
        {
            var participant = await LoginAs("part@test.com", "Participant");

            HttpResponseMessage response = await participant.Client.PostAsJsonAsync("/api/enrolments/1/results",
                new { finishTime = "00:48:32", position = 1 });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task MyResults_AsOrganiser_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.GetAsync("/api/results/my");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ViewOtherUsersProfile_Returns403()
        {
            var first = await LoginAs("first@test.com", "Participant");
            var second = await LoginAs("second@test.com", "Participant");

            HttpResponseMessage response = await first.Client.GetAsync($"/api/users/{second.UserId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

    }//end of class
}//end of namespace