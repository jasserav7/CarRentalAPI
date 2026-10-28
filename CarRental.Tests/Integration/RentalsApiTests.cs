using System.Net;
using System.Net.Http.Json;
using CarRental.Api.Dto;

namespace CarRental.Tests.Integration
{
    public class RentalsApiTests : IClassFixture<ApiFactory>
    {
        private record CreatedRental(int Id, decimal TotalPrice);

        private readonly HttpClient _client;

        public RentalsApiTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        // Each test books its own car so tests stay independent of each other
        private async Task<(UserDto User, CarDto Car)> CreateUserAndCarAsync(decimal price = 50m)
        {
            var user = await (await _client.PostAsJsonAsync("/api/users",
                new UserDto { Username = "renter", Email = "renter@example.com" })).Content.ReadFromJsonAsync<UserDto>();
            var car = await (await _client.PostAsJsonAsync("/api/cars",
                new CarDto { Brand = "Kia", Model = "Rio", Price = price, Year = DateTime.Now.Year })).Content.ReadFromJsonAsync<CarDto>();
            return (user!, car!);
        }

        private Task<HttpResponseMessage> RentAsync(int userId, int carId, DateTime start, DateTime end) =>
            _client.PostAsJsonAsync("/api/rentals", new RentalDto { UserId = userId, CarId = carId, StartDate = start, EndDate = end });

        private static readonly DateTime Start = new(2031, 1, 10, 10, 0, 0);

        [Fact]
        public async Task Post_CreatesRental_WithTotalPrice()
        {
            var (user, car) = await CreateUserAndCarAsync(price: 50m);

            var response = await RentAsync(user.Id, car.Id, Start, Start.AddDays(4));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<CreatedRental>();
            Assert.Equal(200m, created!.TotalPrice);
            Assert.True(created.Id > 0);
        }

        [Fact]
        public async Task Post_Returns400_ForInvalidPeriod()
        {
            var (user, car) = await CreateUserAndCarAsync();

            var reversed = await RentAsync(user.Id, car.Id, Start.AddDays(2), Start);
            var tooShort = await RentAsync(user.Id, car.Id, Start, Start.AddHours(3));

            Assert.Equal(HttpStatusCode.BadRequest, reversed.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);
        }

        [Fact]
        public async Task Post_Returns404_WhenUserOrCarDoNotExist()
        {
            var (user, car) = await CreateUserAndCarAsync();

            var noUser = await RentAsync(999999, car.Id, Start, Start.AddDays(2));
            var noCar = await RentAsync(user.Id, 999999, Start, Start.AddDays(2));

            Assert.Equal(HttpStatusCode.NotFound, noUser.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, noCar.StatusCode);
        }

        [Fact]
        public async Task Post_Returns409_WhenCarIsAlreadyRented_ButAllowsBackToBack()
        {
            var (user, car) = await CreateUserAndCarAsync();
            Assert.Equal(HttpStatusCode.OK, (await RentAsync(user.Id, car.Id, Start, Start.AddDays(4))).StatusCode);

            var overlapping = await RentAsync(user.Id, car.Id, Start.AddDays(2), Start.AddDays(6));
            var backToBack = await RentAsync(user.Id, car.Id, Start.AddDays(4), Start.AddDays(6));

            Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
            Assert.Equal(HttpStatusCode.OK, backToBack.StatusCode);
        }

        [Fact]
        public async Task Get_ById_ReturnsDetail_Or404()
        {
            var (user, car) = await CreateUserAndCarAsync();
            var created = await (await RentAsync(user.Id, car.Id, Start, Start.AddDays(2))).Content.ReadFromJsonAsync<CreatedRental>();

            var detail = await _client.GetFromJsonAsync<RentalDetailDto>($"/api/rentals/{created!.Id}");
            var missing = await _client.GetAsync("/api/rentals/999999");

            Assert.Equal("renter", detail!.Username);
            Assert.Equal("Kia", detail.CarBrand);
            Assert.Equal(created.TotalPrice, detail.TotalPrice);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }

        [Fact]
        public async Task Get_FiltersByCarAndDateWindow()
        {
            var (user, car) = await CreateUserAndCarAsync();
            await RentAsync(user.Id, car.Id, Start, Start.AddDays(4));

            var byCar = await _client.GetFromJsonAsync<List<RentalDetailDto>>($"/api/rentals?carId={car.Id}");
            var inWindow = await _client.GetFromJsonAsync<List<RentalDetailDto>>(
                $"/api/rentals?carId={car.Id}&from={Start.AddDays(1):yyyy-MM-dd}&to={Start.AddDays(2):yyyy-MM-dd}");
            var outsideWindow = await _client.GetFromJsonAsync<List<RentalDetailDto>>(
                $"/api/rentals?carId={car.Id}&from={Start.AddDays(10):yyyy-MM-dd}");

            Assert.Single(byCar!);
            Assert.Single(inWindow!);
            Assert.Empty(outsideWindow!);
        }

        [Fact]
        public async Task Get_Returns400_WhenFromIsNotBeforeTo()
        {
            var response = await _client.GetAsync("/api/rentals?from=2031-02-01&to=2031-01-01");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
