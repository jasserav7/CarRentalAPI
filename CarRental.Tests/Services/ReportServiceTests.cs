using CarRental.Api.Services;

namespace CarRental.Tests.Services
{
    public class ReportServiceTests : IDisposable
    {
        private static readonly DateTime Jan10 = new(2027, 1, 10, 10, 0, 0);

        private readonly TestDb _db = new();

        public void Dispose() => _db.Dispose();

        private ReportService CreateService() => new(_db.CreateContext());

        [Fact]
        public async Task Summary_IsEmpty_WhenThereAreNoRentals()
        {
            var summary = await CreateService().GetSummaryAsync();

            Assert.Equal(0, summary.TotalRentals);
            Assert.Equal(0m, summary.TotalRevenue);
            Assert.Null(summary.MostRentedCar);
        }

        [Fact]
        public async Task RevenueByCar_IsEmpty_WhenThereAreNoRentals()
        {
            Assert.Empty(await CreateService().GetRevenueByCarAsync());
        }

        [Fact]
        public async Task Summary_TotalsRentalsAndRevenue_AndPicksMostRentedCar()
        {
            var user = _db.AddUser();
            var toyota = _db.AddCar("Toyota", "Corolla", 50m);
            var mazda = _db.AddCar("Mazda", "3", 100m);
            _db.AddRental(user.Id, toyota.Id, Jan10, Jan10.AddDays(1), 50m);
            _db.AddRental(user.Id, toyota.Id, Jan10.AddDays(5), Jan10.AddDays(7), 100m);
            _db.AddRental(user.Id, mazda.Id, Jan10, Jan10.AddDays(1), 100m);

            var summary = await CreateService().GetSummaryAsync();

            Assert.Equal(3, summary.TotalRentals);
            Assert.Equal(250m, summary.TotalRevenue);
            Assert.Equal(toyota.Id, summary.MostRentedCar!.CarId);
            Assert.Equal(2, summary.MostRentedCar.RentalCount);
        }

        [Fact]
        public async Task Summary_BreaksRentalCountTies_ByRevenue()
        {
            var user = _db.AddUser();
            var cheap = _db.AddCar("Toyota", "Corolla", 50m);
            var pricey = _db.AddCar("Mercedes-Benz", "E 320", 90m);
            _db.AddRental(user.Id, cheap.Id, Jan10, Jan10.AddDays(1), 50m);
            _db.AddRental(user.Id, pricey.Id, Jan10, Jan10.AddDays(1), 90m);

            var summary = await CreateService().GetSummaryAsync();

            Assert.Equal(pricey.Id, summary.MostRentedCar!.CarId);
        }

        [Fact]
        public async Task RevenueByCar_AggregatesPerCar_OrderedByRevenueDescending()
        {
            var user = _db.AddUser();
            var toyota = _db.AddCar("Toyota", "Corolla", 50m);
            var mazda = _db.AddCar("Mazda", "3", 100m);
            _db.AddCar("Unused", "Car", 10m); // never rented: must not appear
            _db.AddRental(user.Id, toyota.Id, Jan10, Jan10.AddDays(1), 50m);
            _db.AddRental(user.Id, toyota.Id, Jan10.AddDays(5), Jan10.AddDays(8), 150m);
            _db.AddRental(user.Id, mazda.Id, Jan10, Jan10.AddDays(5), 500m);

            var report = await CreateService().GetRevenueByCarAsync();

            Assert.Equal(2, report.Count);
            Assert.Equal("Mazda", report[0].Brand);
            Assert.Equal(500m, report[0].Revenue);
            Assert.Equal(5, report[0].TotalDays);
            Assert.Equal("Toyota", report[1].Brand);
            Assert.Equal(2, report[1].RentalCount);
            Assert.Equal(4, report[1].TotalDays);
            Assert.Equal(200m, report[1].Revenue);
        }
    }
}
