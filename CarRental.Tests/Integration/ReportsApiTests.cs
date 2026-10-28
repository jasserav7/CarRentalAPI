using System.Net.Http.Json;
using CarRental.Api.Dto;

namespace CarRental.Tests.Integration
{
    public class ReportsApiTests
    {
        private static readonly DateTime Start = new(2032, 1, 10, 10, 0, 0);

        // Each test gets its own factory so the totals are exact (the seed data has no rentals)

        [Fact]
        public async Task Summary_IsEmpty_WhenThereAreNoRentals()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var summary = await client.GetFromJsonAsync<SummaryReportDto>("/api/reports/summary");
            var byCar = await client.GetFromJsonAsync<List<RevenueByCarDto>>("/api/reports/revenue-by-car");

            Assert.Equal(0, summary!.TotalRentals);
            Assert.Equal(0m, summary.TotalRevenue);
            Assert.Null(summary.MostRentedCar);
            Assert.Empty(byCar!);
        }

        [Fact]
        public async Task Reports_ReflectCreatedRentals()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();
            // Seeded cars: 1 = Toyota Corolla (45/day), 2 = Mazda 3 (50/day); seeded user 1
            async Task Rent(int carId, int days, int offsetDays) =>
                (await client.PostAsJsonAsync("/api/rentals", new RentalDto
                {
                    UserId = 1,
                    CarId = carId,
                    StartDate = Start.AddDays(offsetDays),
                    EndDate = Start.AddDays(offsetDays + days),
                })).EnsureSuccessStatusCode();
            await Rent(carId: 1, days: 2, offsetDays: 0);   // 90
            await Rent(carId: 1, days: 1, offsetDays: 5);   // 45
            await Rent(carId: 2, days: 4, offsetDays: 0);   // 200

            var summary = await client.GetFromJsonAsync<SummaryReportDto>("/api/reports/summary");
            var byCar = await client.GetFromJsonAsync<List<RevenueByCarDto>>("/api/reports/revenue-by-car");

            Assert.Equal(3, summary!.TotalRentals);
            Assert.Equal(335m, summary.TotalRevenue);
            Assert.Equal(1, summary.MostRentedCar!.CarId);
            Assert.Equal(2, summary.MostRentedCar.RentalCount);

            Assert.Equal(2, byCar!.Count);
            Assert.Equal(2, byCar[0].CarId); // highest revenue first
            Assert.Equal(200m, byCar[0].Revenue);
            Assert.Equal(4, byCar[0].TotalDays);
            Assert.Equal(135m, byCar[1].Revenue);
            Assert.Equal(3, byCar[1].TotalDays);
        }
    }
}
