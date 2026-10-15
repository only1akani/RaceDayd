using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //tests for event management by organisers
    public class EventTests : ApiTestBase
    {//start of class

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.PostAsJsonAsync("/api/events", EventBody());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_MissingName_Returns400()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.PostAsJsonAsync("/api/events",
                new { eventDate = "2027-01-01T06:00:00", location = "Johannesburg" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEvents_WithoutLogin_Returns200()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            await CreateEvent(organiser.Client);

            //a brand new client has no session, events are public
            HttpResponseMessage response = await NewClient().GetAsync("/api/events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetArrayLength());
        }

        [Fact]
        public async Task GetEvent_UnknownId_Returns404()
        {
            HttpResponseMessage response = await NewClient().GetAsync("/api/events/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_Owner_Returns200AndSavesChange()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage update = await organiser.Client.PutAsJsonAsync($"/api/events/{eventId}", EventBody("Renamed Marathon"));
            HttpResponseMessage get = await NewClient().GetAsync($"/api/events/{eventId}");

            Assert.Equal(HttpStatusCode.OK, update.StatusCode);
            JsonElement body = await get.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Renamed Marathon", body.GetProperty("eventName").GetString());
        }

        [Fact]
        public async Task UpdateEvent_OtherOrganiser_Returns403()
        {
            var owner = await LoginAs("owner@test.com", "Organiser");
            var other = await LoginAs("other@test.com", "Organiser");
            int eventId = await CreateEvent(owner.Client);

            HttpResponseMessage response = await other.Client.PutAsJsonAsync($"/api/events/{eventId}", EventBody("Stolen"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_UnknownId_Returns404()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.PutAsJsonAsync("/api/events/999", EventBody());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_Owner_Returns200AndEventIsGone()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage delete = await organiser.Client.DeleteAsync($"/api/events/{eventId}");
            HttpResponseMessage get = await NewClient().GetAsync($"/api/events/{eventId}");

            Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_OtherOrganiser_Returns403()
        {
            var owner = await LoginAs("owner@test.com", "Organiser");
            var other = await LoginAs("other@test.com", "Organiser");
            int eventId = await CreateEvent(owner.Client);

            HttpResponseMessage response = await other.Client.DeleteAsync($"/api/events/{eventId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

    }//end of class
}//end of namespace