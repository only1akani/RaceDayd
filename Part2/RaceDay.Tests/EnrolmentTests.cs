using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //tests for participants entering events
    public class EnrolmentTests : ApiTestBase
    {//start of class

        //creates an organiser, an event and a category, returns the category id
        private async Task<(HttpClient Organiser, int EventId, int CategoryId)> SetupCategory(int maxParticipants = 100)
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);
            int categoryId = await CreateCategory(organiser.Client, eventId, maxParticipants);
            return (organiser.Client, eventId, categoryId);
        }

        [Fact]
        public async Task Enrol_AsParticipant_Returns201WithPendingStatus()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");

            HttpResponseMessage response = await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Pending", body.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Enrol_IsRecordedInDatabase()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");

            await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            //check the link between participant, category and event in the database
            using (IServiceScope scope = Factory.Services.CreateScope())
            {
                RaceDayDbContext db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                Enrolment enrolment = db.Enrolments.Single(e => e.ParticipantId == participant.UserId);

                Assert.Equal(setup.CategoryId, enrolment.CategoryId);
                Assert.Equal("Pending", enrolment.Status);
                Assert.Equal(setup.EventId, db.Categories.Single(c => c.CategoryId == enrolment.CategoryId).EventId);
            }
        }

        [Fact]
        public async Task Enrol_SameCategoryTwice_Returns409()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            HttpResponseMessage response = await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_UnknownCategory_Returns404()
        {
            var participant = await LoginAs("part@test.com", "Participant");

            HttpResponseMessage response = await participant.Client.PostAsync("/api/categories/999/enrolments", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_FullCategory_Returns409()
        {
            var setup = await SetupCategory(maxParticipants: 1);
            var first = await LoginAs("first@test.com", "Participant");
            var second = await LoginAs("second@test.com", "Participant");
            await first.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            HttpResponseMessage response = await second.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task MyEnrolments_ReturnsOnlyOwnEnrolments()
        {
            var setup = await SetupCategory();
            var first = await LoginAs("first@test.com", "Participant");
            var second = await LoginAs("second@test.com", "Participant");
            await first.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            HttpResponseMessage firstList = await first.Client.GetAsync("/api/enrolments/my");
            HttpResponseMessage secondList = await second.Client.GetAsync("/api/enrolments/my");

            JsonElement firstBody = await firstList.Content.ReadFromJsonAsync<JsonElement>();
            JsonElement secondBody = await secondList.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, firstBody.GetArrayLength());
            Assert.Equal(0, secondBody.GetArrayLength());
        }

        [Fact]
        public async Task EventEnrolments_Owner_Returns200()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);

            HttpResponseMessage response = await setup.Organiser.GetAsync($"/api/events/{setup.EventId}/enrolments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetArrayLength());
        }

        [Fact]
        public async Task EventEnrolments_OtherOrganiser_Returns403()
        {
            var setup = await SetupCategory();
            var other = await LoginAs("other@test.com", "Organiser");

            HttpResponseMessage response = await other.Client.GetAsync($"/api/events/{setup.EventId}/enrolments");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_OwnEnrolment_Returns200()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            HttpResponseMessage enrol = await participant.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);
            int enrolmentId = await ReadInt(enrol, "enrolmentId");

            HttpResponseMessage response = await participant.Client.DeleteAsync($"/api/enrolments/{enrolmentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_SomeoneElsesEnrolment_Returns403()
        {
            var setup = await SetupCategory();
            var owner = await LoginAs("owner@test.com", "Participant");
            var other = await LoginAs("other@test.com", "Participant");
            HttpResponseMessage enrol = await owner.Client.PostAsync($"/api/categories/{setup.CategoryId}/enrolments", null);
            int enrolmentId = await ReadInt(enrol, "enrolmentId");

            HttpResponseMessage response = await other.Client.DeleteAsync($"/api/enrolments/{enrolmentId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

    }//end of class
}//end of namespace