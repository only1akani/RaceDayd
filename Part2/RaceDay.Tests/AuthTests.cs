using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    //tests for registering and logging in
    public class AuthTests : ApiTestBase
    {//start of class

        [Fact]
        public async Task Register_ValidOrganiser_Returns201()
        {
            HttpClient client = NewClient();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("org@test.com", "Organiser"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_ValidParticipant_Returns201()
        {
            HttpClient client = NewClient();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("part@test.com", "Participant"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            HttpClient client = NewClient();
            await client.PostAsJsonAsync("/api/auth/register", RegisterBody("dup@test.com", "Participant"));

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("dup@test.com", "Participant"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            HttpClient client = NewClient();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("admin@test.com", "Admin"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_MissingFields_Returns400()
        {
            HttpClient client = NewClient();

            //only an email, everything else is missing
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new { email = "missing@test.com" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_PasswordIsHashedInDatabase()
        {
            HttpClient client = NewClient();
            await client.PostAsJsonAsync("/api/auth/register", RegisterBody("hash@test.com", "Participant"));

            //look in the database directly
            using (IServiceScope scope = Factory.Services.CreateScope())
            {
                RaceDayDbContext db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                User user = db.Users.Single(u => u.Email == "hash@test.com");

                Assert.NotEqual("Pass123!", user.PasswordHash);
                Assert.StartsWith("$2", user.PasswordHash);
            }
        }

        [Fact]
        public async Task Login_CorrectDetails_Returns200AndRole()
        {
            HttpClient client = NewClient();
            await client.PostAsJsonAsync("/api/auth/register", RegisterBody("login@test.com", "Organiser"));

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { email = "login@test.com", password = "Pass123!" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Organiser", body.GetProperty("role").GetString());
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            HttpClient client = NewClient();
            await client.PostAsJsonAsync("/api/auth/register", RegisterBody("wrong@test.com", "Participant"));

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { email = "wrong@test.com", password = "WrongPass1" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_UnknownEmail_Returns401()
        {
            HttpClient client = NewClient();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { email = "nobody@test.com", password = "Pass123!" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_ClearsTheSession()
        {
            var user = await LoginAs("logout@test.com", "Participant");

            //first logout works because the user is logged in
            HttpResponseMessage first = await user.Client.PostAsync("/api/auth/logout", null);
            //second logout fails because the session was cleared
            HttpResponseMessage second = await user.Client.PostAsync("/api/auth/logout", null);

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
        }

    }//end of class
}//end of namespace