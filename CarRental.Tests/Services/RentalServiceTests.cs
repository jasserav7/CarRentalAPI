using CarRental.Api.Dto;
using CarRental.Api.Services;

namespace CarRental.Tests.Services
{
    public class RentalServiceTests : IDisposable
    {
        private static readonly DateTime Jan10 = new(2027, 1, 10, 10, 0, 0);

        private readonly TestDb _db = new();

        public void Dispose() => _db.Dispose();

        private RentalService CreateService() => new(_db.CreateContext());

        private static RentalDto Dto(int userId, int carId, DateTime start, DateTime end) =>
            new() { UserId = userId, CarId = carId, StartDate = start, EndDate = end };

        [Fact]
        public async Task CreateRental_CalculatesTotalPrice_AsDaysTimesDailyPrice()
        {
            var user = _db.AddUser();
            var car = _db.AddCar(price: 50m);

            var result = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10, Jan10.AddDays(4)));

            Assert.True(result.Success);
            Assert.Equal(200m, result.TotalPrice);
            var stored = await CreateService().GetRentalAsync(result.RentalId);
            Assert.Equal(200m, stored!.TotalPrice);
        }

        [Fact]
        public async Task CreateRental_Fails_WhenStartIsNotBeforeEnd()
        {
            var user = _db.AddUser();
            var car = _db.AddCar();

            var same = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10, Jan10));
            var reversed = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10.AddDays(2), Jan10));

            Assert.Equal(RentalError.InvalidPeriod, same.Error);
            Assert.Equal(RentalError.InvalidPeriod, reversed.Error);
        }

        [Fact]
        public async Task CreateRental_Fails_WhenShorterThanOneDay()
        {
            var user = _db.AddUser();
            var car = _db.AddCar();

            var result = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10, Jan10.AddHours(5)));

            Assert.Equal(RentalError.InvalidPeriod, result.Error);
        }

        [Fact]
        public async Task CreateRental_Fails_WhenUserDoesNotExist()
        {
            var car = _db.AddCar();

            var result = await CreateService().CreateRentalAsync(Dto(999, car.Id, Jan10, Jan10.AddDays(2)));

            Assert.Equal(RentalError.UserNotFound, result.Error);
        }

        [Fact]
        public async Task CreateRental_Fails_WhenCarDoesNotExist()
        {
            var user = _db.AddUser();

            var result = await CreateService().CreateRentalAsync(Dto(user.Id, 999, Jan10, Jan10.AddDays(2)));

            Assert.Equal(RentalError.CarNotFound, result.Error);
        }

        // Existing rental: Jan 10 -> Jan 14
        [Theory]
        [InlineData(0, 2)]   // same start, ends inside
        [InlineData(2, 6)]   // starts inside, ends after
        [InlineData(-2, 2)]  // starts before, ends inside
        [InlineData(-2, 6)]  // fully contains the existing rental
        [InlineData(1, 3)]   // fully inside the existing rental
        public async Task CreateRental_Fails_WhenPeriodOverlapsExistingRental(int startOffsetDays, int endOffsetDays)
        {
            var user = _db.AddUser();
            var car = _db.AddCar();
            _db.AddRental(user.Id, car.Id, Jan10, Jan10.AddDays(4), 200m);

            var result = await CreateService().CreateRentalAsync(
                Dto(user.Id, car.Id, Jan10.AddDays(startOffsetDays), Jan10.AddDays(endOffsetDays)));

            Assert.Equal(RentalError.CarNotAvailable, result.Error);
        }

        [Fact]
        public async Task CreateRental_Succeeds_WhenBackToBackWithExistingRental()
        {
            var user = _db.AddUser();
            var car = _db.AddCar();
            _db.AddRental(user.Id, car.Id, Jan10, Jan10.AddDays(4), 200m);

            var after = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10.AddDays(4), Jan10.AddDays(6)));
            var before = await CreateService().CreateRentalAsync(Dto(user.Id, car.Id, Jan10.AddDays(-3), Jan10));

            Assert.True(after.Success);
            Assert.True(before.Success);
        }

        [Fact]
        public async Task CreateRental_Succeeds_WhenSamePeriodButDifferentCar()
        {
            var user = _db.AddUser();
            var carA = _db.AddCar("Toyota", "Corolla");
            var carB = _db.AddCar("Mazda", "3");
            _db.AddRental(user.Id, carA.Id, Jan10, Jan10.AddDays(4), 200m);

            var result = await CreateService().CreateRentalAsync(Dto(user.Id, carB.Id, Jan10, Jan10.AddDays(4)));

            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetRentals_FiltersByUserAndCar()
        {
            var u1 = _db.AddUser("u1", "u1@example.com");
            var u2 = _db.AddUser("u2", "u2@example.com");
            var c1 = _db.AddCar("Toyota", "Corolla");
            var c2 = _db.AddCar("Mazda", "3");
            _db.AddRental(u1.Id, c1.Id, Jan10, Jan10.AddDays(1), 50m);
            _db.AddRental(u1.Id, c2.Id, Jan10, Jan10.AddDays(1), 50m);
            _db.AddRental(u2.Id, c1.Id, Jan10.AddDays(5), Jan10.AddDays(6), 50m);

            var service = CreateService();

            Assert.Equal(3, (await service.GetRentalsAsync(null, null, null, null)).Count);
            Assert.Equal(2, (await service.GetRentalsAsync(u1.Id, null, null, null)).Count);
            Assert.Equal(2, (await service.GetRentalsAsync(null, c1.Id, null, null)).Count);
            Assert.Single(await service.GetRentalsAsync(u1.Id, c1.Id, null, null));
        }

        [Fact]
        public async Task GetRentals_FiltersByDateWindow_UsingOverlap()
        {
            var user = _db.AddUser();
            var car = _db.AddCar();
            _db.AddRental(user.Id, car.Id, Jan10, Jan10.AddDays(4), 200m); // Jan 10 -> 14
            var service = CreateService();

            // Window inside the rental, window crossing its edge: both overlap
            Assert.Single(await service.GetRentalsAsync(null, null, Jan10.AddDays(1), Jan10.AddDays(2)));
            Assert.Single(await service.GetRentalsAsync(null, null, Jan10.AddDays(3), Jan10.AddDays(9)));
            // Windows that only touch the edge or sit outside: no overlap
            Assert.Empty(await service.GetRentalsAsync(null, null, Jan10.AddDays(4), null));
            Assert.Empty(await service.GetRentalsAsync(null, null, null, Jan10));
        }

        [Fact]
        public async Task GetRentals_IncludesUserAndCarDetails_OrderedByStartDate()
        {
            var user = _db.AddUser("maria", "maria@example.com");
            var car = _db.AddCar("BMW", "M3", 60m);
            _db.AddRental(user.Id, car.Id, Jan10.AddDays(10), Jan10.AddDays(11), 60m);
            _db.AddRental(user.Id, car.Id, Jan10, Jan10.AddDays(1), 60m);

            var rentals = await CreateService().GetRentalsAsync(null, null, null, null);

            Assert.Equal(Jan10, rentals[0].StartDate);
            Assert.Equal("maria", rentals[0].Username);
            Assert.Equal("BMW", rentals[0].CarBrand);
            Assert.Equal("M3", rentals[0].CarModel);
        }

        [Fact]
        public async Task GetRental_ReturnsNull_WhenMissing()
        {
            Assert.Null(await CreateService().GetRentalAsync(999));
        }
    }
}
