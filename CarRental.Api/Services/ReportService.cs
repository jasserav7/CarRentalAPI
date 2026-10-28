using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Data;
using CarRental.Api.Dto;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SummaryReportDto> GetSummaryAsync()
        {
            var byCar = await GetRevenueByCarAsync();
            var top = byCar
                .OrderByDescending(c => c.RentalCount)
                .ThenByDescending(c => c.Revenue)
                .FirstOrDefault();

            return new SummaryReportDto
            {
                TotalRentals = byCar.Sum(c => c.RentalCount),
                TotalRevenue = byCar.Sum(c => c.Revenue),
                MostRentedCar = top == null
                    ? null
                    : new MostRentedCarDto
                    {
                        CarId = top.CarId,
                        Brand = top.Brand,
                        Model = top.Model,
                        RentalCount = top.RentalCount,
                    },
            };
        }

        public async Task<List<RevenueByCarDto>> GetRevenueByCarAsync()
        {
            // Days are computed in memory: the dataset is small and it avoids provider-specific date math
            var rentals = await _context.Rentals
                .AsNoTracking()
                .Select(r => new
                {
                    r.CarId,
                    r.Car.Brand,
                    r.Car.Model,
                    r.StartDate,
                    r.EndDate,
                    r.Price,
                })
                .ToListAsync();

            return rentals
                .GroupBy(r => new { r.CarId, r.Brand, r.Model })
                .Select(g => new RevenueByCarDto
                {
                    CarId = g.Key.CarId,
                    Brand = g.Key.Brand,
                    Model = g.Key.Model,
                    RentalCount = g.Count(),
                    TotalDays = g.Sum(r => (r.EndDate - r.StartDate).Days),
                    Revenue = g.Sum(r => r.Price),
                })
                .OrderByDescending(c => c.Revenue)
                .ToList();
        }
    }
}
