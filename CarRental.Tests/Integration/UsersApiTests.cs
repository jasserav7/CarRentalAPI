using System.Net;
using System.Net.Http.Json;
using CarRental.Api.Dto;

namespace CarRental.Tests.Integration
{
    public class UsersApiTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public UsersApiTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<UserDto> CreateUserAsync(string username = "tester", string email = "tester@example.com")
        {
            var response = await _client.PostAsJsonAsync("/api/users", new UserDto { Username = username, Email = email });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<UserDto>())!;
        }

        [Fact]
        public async Task Get_ReturnsSeededUsers()
        {
            var users = await _client.GetFromJsonAsync<List<UserDto>>("/api/users");

            Assert.NotNull(users);
            Assert.Contains(users, u => u.Username == "John Doe");
        }

        [Fact]
        public async Task Post_CreatesUser_AndReturnsLocation()
        {
            var response = await _client.PostAsJsonAsync("/api/users", new UserDto { Username = "ana", Email = "ana@example.com" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.True(created!.Id > 0);
            Assert.EndsWith($"/api/Users/{created.Id}", response.Headers.Location!.ToString());
        }

        [Fact]
        public async Task Post_RejectsInvalidEmail()
        {
            var response = await _client.PostAsJsonAsync("/api/users", new UserDto { Username = "bad", Email = "not-an-email" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_ById_Returns404_WhenMissing()
        {
            var response = await _client.GetAsync("/api/users/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Put_UpdatesUser()
        {
            var user = await CreateUserAsync("before", "before@example.com");

            var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}",
                new UserDto { Id = user.Id, Username = "after", Email = "after@example.com" });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var fetched = await _client.GetFromJsonAsync<UserDto>($"/api/users/{user.Id}");
            Assert.Equal("after", fetched!.Username);
        }

        [Fact]
        public async Task Put_Returns400_WhenBodyIdDiffersFromRoute()
        {
            var user = await CreateUserAsync();

            var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}",
                new UserDto { Id = user.Id + 1, Username = "x", Email = "x@example.com" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_Returns404_WhenMissing()
        {
            var response = await _client.PutAsJsonAsync("/api/users/999999",
                new UserDto { Id = 999999, Username = "x", Email = "x@example.com" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesUser_ThenReturns404()
        {
            var user = await CreateUserAsync();

            var first = await _client.DeleteAsync($"/api/users/{user.Id}");
            var second = await _client.DeleteAsync($"/api/users/{user.Id}");

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        }

        [Fact]
        public async Task Delete_Returns409_WhenUserHasRentals()
        {
            var user = await CreateUserAsync();
            var rental = await _client.PostAsJsonAsync("/api/rentals", new RentalDto
            {
                UserId = user.Id,
                CarId = 1, // seeded car
                StartDate = new DateTime(2030, 3, 1, 10, 0, 0),
                EndDate = new DateTime(2030, 3, 3, 10, 0, 0),
            });
            Assert.Equal(HttpStatusCode.OK, rental.StatusCode);

            var response = await _client.DeleteAsync($"/api/users/{user.Id}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetRentals_ReturnsUserHistory_Or404ForUnknownUser()
        {
            var user = await CreateUserAsync();
            await _client.PostAsJsonAsync("/api/rentals", new RentalDto
            {
                UserId = user.Id,
                CarId = 2, // seeded car
                StartDate = new DateTime(2030, 4, 1, 10, 0, 0),
                EndDate = new DateTime(2030, 4, 2, 10, 0, 0),
            });

            var history = await _client.GetFromJsonAsync<List<RentalDetailDto>>($"/api/users/{user.Id}/rentals");
            var missing = await _client.GetAsync("/api/users/999999/rentals");

            Assert.Single(history!);
            Assert.Equal(user.Id, history![0].UserId);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }
    }
}
