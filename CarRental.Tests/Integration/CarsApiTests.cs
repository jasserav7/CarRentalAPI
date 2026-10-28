using System.Net;
using System.Net.Http.Json;
using CarRental.Api.Dto;

namespace CarRental.Tests.Integration
{
    public class CarsApiTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public CarsApiTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static CarDto NewCar(int? year = null) =>
            new() { Brand = "Kia", Model = "Rio", Price = 40m, Year = year ?? DateTime.Now.Year };

        [Fact]
        public async Task Get_ReturnsSeededCars()
        {
            var cars = await _client.GetFromJsonAsync<List<CarDto>>("/api/cars");

            Assert.NotNull(cars);
            Assert.Contains(cars, c => c.Brand == "Toyota");
        }

        [Fact]
        public async Task Post_CreatesCar()
        {
            var response = await _client.PostAsJsonAsync("/api/cars", NewCar());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<CarDto>();
            Assert.True(created!.Id > 0);
        }

        [Fact]
        public async Task Post_Returns400_WhenCarIsOlderThanFiveYears()
        {
            var response = await _client.PostAsJsonAsync("/api/cars", NewCar(DateTime.Now.Year - 6));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Accepts_CarExactlyFiveYearsOld()
        {
            var response = await _client.PostAsJsonAsync("/api/cars", NewCar(DateTime.Now.Year - 5));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Put_UpdatesCar_AndEnforcesTheAgeRule()
        {
            var created = await (await _client.PostAsJsonAsync("/api/cars", NewCar())).Content.ReadFromJsonAsync<CarDto>();
            created!.Price = 99m;

            var ok = await _client.PutAsJsonAsync($"/api/cars/{created.Id}", created);
            created.Year = DateTime.Now.Year - 10;
            var tooOld = await _client.PutAsJsonAsync($"/api/cars/{created.Id}", created);

            Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, tooOld.StatusCode);
            var fetched = await _client.GetFromJsonAsync<CarDto>($"/api/cars/{created.Id}");
            Assert.Equal(99m, fetched!.Price);
        }

        [Fact]
        public async Task Put_Returns400_WhenBodyIdDiffersFromRoute_And404WhenMissing()
        {
            var mismatch = await _client.PutAsJsonAsync("/api/cars/1", new CarDto { Id = 2, Brand = "x", Model = "x", Price = 1m, Year = DateTime.Now.Year });
            var missing = await _client.PutAsJsonAsync("/api/cars/999999", new CarDto { Id = 999999, Brand = "x", Model = "x", Price = 1m, Year = DateTime.Now.Year });

            Assert.Equal(HttpStatusCode.BadRequest, mismatch.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesCar_ThenReturns404()
        {
            var created = await (await _client.PostAsJsonAsync("/api/cars", NewCar())).Content.ReadFromJsonAsync<CarDto>();

            var first = await _client.DeleteAsync($"/api/cars/{created!.Id}");
            var second = await _client.DeleteAsync($"/api/cars/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        }
    }
}
