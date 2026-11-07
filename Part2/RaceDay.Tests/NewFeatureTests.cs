using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //tests for the Part 3 additions: event type, uploads, enrolment status, dashboard and finishers
    public class NewFeatureTests : ApiTestBase
    {//start of class

        //builds a small fake image upload
        private static MultipartFormDataContent ImageForm(string fileName = "pic.png", string contentType = "image/png")
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            ByteArrayContent image = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            image.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(image, "file", fileName);
            return form;
        }

        //creates an organiser, an event and a category, and returns the ids
        private async Task<(HttpClient Organiser, int EventId, int CategoryId)> SetupCategory()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);
            int categoryId = await CreateCategory(organiser.Client, eventId);
            return (organiser.Client, eventId, categoryId);
        }

        //enrols the client in the category and returns the enrolment id
        private async Task<int> Enrol(HttpClient participant, int categoryId)
        {
            HttpResponseMessage response = await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            response.EnsureSuccessStatusCode();
            return await ReadInt(response, "enrolmentId");
        }

        [Fact]
        public async Task CreateEvent_WithTypeAndDistance_IsSaved()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage create = await organiser.Client.PostAsJsonAsync("/api/events", new
            {
                eventName = "Cycle Classic",
                eventDescription = "Road cycle",
                eventDate = "2027-03-01T06:00:00",
                location = "Pretoria",
                eventType = "cycle",
                distance = 94.5
            });
            int eventId = await ReadInt(create, "eventId");
            HttpResponseMessage get = await NewClient().GetAsync($"/api/events/{eventId}");

            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            JsonElement body = await get.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Cycle", body.GetProperty("eventType").GetString());
            Assert.Equal(94.5, body.GetProperty("distance").GetDouble());
        }

        [Fact]
        public async Task CreateEvent_InvalidType_Returns400()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.Client.PostAsJsonAsync("/api/events", new
            {
                eventName = "Swim Gala",
                eventDate = "2027-03-01T06:00:00",
                eventType = "Swim"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEvents_FilterByType_ReturnsOnlyThatType()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            await CreateEvent(organiser.Client);
            await organiser.Client.PostAsJsonAsync("/api/events", new
            {
                eventName = "Cycle Classic",
                eventDate = "2027-03-01T06:00:00",
                eventType = "Cycle"
            });

            HttpResponseMessage response = await NewClient().GetAsync("/api/events?type=Cycle");

            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetArrayLength());
            Assert.Equal("Cycle Classic", body[0].GetProperty("eventName").GetString());
        }

        [Fact]
        public async Task UploadBanner_Owner_Returns200WithUrl()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage response = await organiser.Client.PostAsync($"/api/events/{eventId}/banner", ImageForm());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Contains("event-banners", body.GetProperty("bannerUrl").GetString());
        }

        [Fact]
        public async Task UploadBanner_OtherOrganiser_Returns403()
        {
            var owner = await LoginAs("owner@test.com", "Organiser");
            var other = await LoginAs("other@test.com", "Organiser");
            int eventId = await CreateEvent(owner.Client);

            HttpResponseMessage response = await other.Client.PostAsync($"/api/events/{eventId}/banner", ImageForm());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UploadBanner_AsParticipant_Returns403()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            var participant = await LoginAs("part@test.com", "Participant");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage response = await participant.Client.PostAsync($"/api/events/{eventId}/banner", ImageForm());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UploadBanner_NotAnImage_Returns400()
        {
            var organiser = await LoginAs("org@test.com", "Organiser");
            int eventId = await CreateEvent(organiser.Client);

            HttpResponseMessage response = await organiser.Client.PostAsync($"/api/events/{eventId}/banner", ImageForm("notes.txt", "text/plain"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UploadProfilePicture_Own_Returns200AndShowsOnProfile()
        {
            var participant = await LoginAs("part@test.com", "Participant");

            HttpResponseMessage upload = await participant.Client.PostAsync($"/api/users/{participant.UserId}/profile-picture", ImageForm());
            HttpResponseMessage profile = await participant.Client.GetAsync($"/api/users/{participant.UserId}");

            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
            JsonElement body = await profile.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Contains("profile-pictures", body.GetProperty("profilePictureUrl").GetString());
        }

        [Fact]
        public async Task UploadProfilePicture_OtherUser_Returns403()
        {
            var first = await LoginAs("first@test.com", "Participant");
            var second = await LoginAs("second@test.com", "Participant");

            HttpResponseMessage response = await first.Client.PostAsync($"/api/users/{second.UserId}/profile-picture", ImageForm());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmEnrolment_Owner_Returns200AndParticipantSeesConfirmed()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            int enrolmentId = await Enrol(participant.Client, setup.CategoryId);

            HttpResponseMessage confirm = await setup.Organiser.PutAsJsonAsync($"/api/enrolments/{enrolmentId}/status", new { status = "Confirmed" });
            HttpResponseMessage mine = await participant.Client.GetAsync("/api/enrolments/my");

            Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);
            JsonElement body = await mine.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Confirmed", body[0].GetProperty("status").GetString());
        }

        [Fact]
        public async Task ConfirmEnrolment_OtherOrganiser_Returns403()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            var other = await LoginAs("other@test.com", "Organiser");
            int enrolmentId = await Enrol(participant.Client, setup.CategoryId);

            HttpResponseMessage response = await other.Client.PutAsJsonAsync($"/api/enrolments/{enrolmentId}/status", new { status = "Confirmed" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmEnrolment_AsParticipant_Returns403()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            int enrolmentId = await Enrol(participant.Client, setup.CategoryId);

            HttpResponseMessage response = await participant.Client.PutAsJsonAsync($"/api/enrolments/{enrolmentId}/status", new { status = "Confirmed" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmEnrolment_InvalidStatus_Returns400()
        {
            var setup = await SetupCategory();
            var participant = await LoginAs("part@test.com", "Participant");
            int enrolmentId = await Enrol(participant.Client, setup.CategoryId);

            HttpResponseMessage response = await setup.Organiser.PutAsJsonAsync($"/api/enrolments/{enrolmentId}/status", new { status = "Maybe" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task MyEvents_ReturnsOnlyOwnEventsWithEnrolmentCounts()
        {
            var setup = await SetupCategory();
            var other = await LoginAs("other@test.com", "Organiser");
            await CreateEvent(other.Client);
            var participant = await LoginAs("part@test.com", "Participant");
            await Enrol(participant.Client, setup.CategoryId);

            HttpResponseMessage response = await setup.Organiser.GetAsync("/api/events/mine");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetArrayLength());
            Assert.Equal(1, body[0].GetProperty("enrolmentCount").GetInt32());
        }

        [Fact]
        public async Task MyResults_IncludesTotalFinishers()
        {
            var setup = await SetupCategory();
            var first = await LoginAs("first@test.com", "Participant");
            var second = await LoginAs("second@test.com", "Participant");
            int firstEnrolment = await Enrol(first.Client, setup.CategoryId);
            int secondEnrolment = await Enrol(second.Client, setup.CategoryId);
            await setup.Organiser.PostAsJsonAsync($"/api/enrolments/{firstEnrolment}/results", new { finishTime = "00:48:32", position = 1 });
            await setup.Organiser.PostAsJsonAsync($"/api/enrolments/{secondEnrolment}/results", new { finishTime = "00:52:10", position = 2 });

            HttpResponseMessage response = await first.Client.GetAsync("/api/results/my");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body[0].GetProperty("position").GetInt32());
            Assert.Equal(2, body[0].GetProperty("totalFinishers").GetInt32());
        }

    }//end of class
}//end of namespace