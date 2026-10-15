using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //shared helpers for all the test classes
    //every test gets a fresh factory, so tests never affect each other
    public abstract class ApiTestBase : IDisposable
    {//start of class

        protected readonly RaceDayFactory Factory = new RaceDayFactory();

        //each client keeps its own cookies, so one client = one logged in user
        protected HttpClient NewClient()
        {
            return Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        protected static object RegisterBody(string email, string role)
        {
            return new
            {
                fullName = "Test User",
                email = email,
                password = "Pass123!",
                idNumber = "9001015800089",
                role = role
            };
        }

        //registers a user, logs them in and returns the logged in client and the user id
        protected async Task<(HttpClient Client, int UserId)> LoginAs(string email, string role)
        {//start of method

            HttpClient client = NewClient();

            HttpResponseMessage register = await client.PostAsJsonAsync("/api/auth/register", RegisterBody(email, role));
            register.EnsureSuccessStatusCode();
            int userId = await ReadInt(register, "userId");

            HttpResponseMessage login = await client.PostAsJsonAsync("/api/auth/login", new { email = email, password = "Pass123!" });
            login.EnsureSuccessStatusCode();

            return (client, userId);

        }//end of method

        //reads one number (like eventId) from a JSON response
        protected static async Task<int> ReadInt(HttpResponseMessage response, string property)
        {
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty(property).GetInt32();
        }

        protected static object EventBody(string name = "Test Marathon")
        {
            return new
            {
                eventName = name,
                eventDescription = "Test event",
                eventDate = "2027-01-01T06:00:00",
                location = "Johannesburg"
            };
        }

        //creates an event as the given organiser and returns its id
        protected async Task<int> CreateEvent(HttpClient organiser)
        {
            HttpResponseMessage response = await organiser.PostAsJsonAsync("/api/events", EventBody());
            response.EnsureSuccessStatusCode();
            return await ReadInt(response, "eventId");
        }

        //creates a category for an event and returns its id
        protected async Task<int> CreateCategory(HttpClient organiser, int eventId, int maxParticipants = 100)
        {
            HttpResponseMessage response = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
                new { categoryName = "10km Run", distance = 10, price = 150, maxParticipants = maxParticipants });
            response.EnsureSuccessStatusCode();
            return await ReadInt(response, "categoryId");
        }

        public void Dispose()
        {
            Factory.Dispose();
        }

    }//end of class
}//end of namespace