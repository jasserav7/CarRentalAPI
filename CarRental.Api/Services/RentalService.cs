using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Data;
using CarRental.Api.Dto;
using CarRental.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    public class RentalService : IRentalService
    {
        private readonly AppDbContext _context;

        public RentalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RentalResult> CreateRentalAsync(RentalDto rentalDto)
        {
            if (rentalDto.StartDate >= rentalDto.EndDate)
                return new RentalResult(RentalError.InvalidPeriod, "Invalid rental period. Start date must be before end date.");

            if (!await _context.Users.AnyAsync(u => u.Id == rentalDto.UserId))
                return new RentalResult(RentalError.UserNotFound, $"User with ID {rentalDto.UserId} not found.");

            var car = await _context.Cars.FindAsync(rentalDto.CarId);
            if (car == null)
                return new RentalResult(RentalError.CarNotFound, $"Car with ID {rentalDto.CarId} not found.");

            // Check availability of the car for the requested rental period
            bool isCarAvailable = !await _context.Rentals.AnyAsync(r =>
                r.CarId == rentalDto.CarId
                && (
                    (rentalDto.StartDate >= r.StartDate && rentalDto.StartDate < r.EndDate)
                    || (rentalDto.EndDate > r.StartDate && rentalDto.EndDate <= r.EndDate)
                    || (rentalDto.StartDate <= r.StartDate && rentalDto.EndDate >= r.EndDate)
                )
            );
            if (!isCarAvailable)
                return new RentalResult(RentalError.CarNotAvailable, "The selected car is not available for the requested rental period.");

            var totalDays = (rentalDto.EndDate - rentalDto.StartDate).Days;
            if (totalDays <= 0)
                return new RentalResult(RentalError.InvalidPeriod, "Invalid rental period. The rental must be at least one day.");

            var totalPrice = totalDays * car.Price;

            var rental = new Rental
            {
                UserId = rentalDto.UserId,
                CarId = rentalDto.CarId,
                StartDate = rentalDto.StartDate,
                EndDate = rentalDto.EndDate,
                Price = totalPrice
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return new RentalResult(RentalError.None, RentalId: rental.Id, TotalPrice: totalPrice);
        }

        public async Task<List<RentalDetailDto>> GetRentalsAsync(int? userId, int? carId, DateTime? from, DateTime? to)
        {
            var query = _context.Rentals.AsNoTracking().AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);
            if (carId.HasValue)
                query = query.Where(r => r.CarId == carId.Value);
            if (from.HasValue)
                query = query.Where(r => r.EndDate > from.Value);
            if (to.HasValue)
                query = query.Where(r => r.StartDate < to.Value);

            return await query
                .OrderBy(r => r.StartDate)
                .Select(r => new RentalDetailDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Username = r.User.Username,
                    CarId = r.CarId,
                    CarBrand = r.Car.Brand,
                    CarModel = r.Car.Model,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalPrice = r.Price,
                })
                .ToListAsync();
        }

        public async Task<RentalDetailDto?> GetRentalAsync(int id)
        {
            return await _context.Rentals
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RentalDetailDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Username = r.User.Username,
                    CarId = r.CarId,
                    CarBrand = r.Car.Brand,
                    CarModel = r.Car.Model,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalPrice = r.Price,
                })
                .FirstOrDefaultAsync();
        }
    }
}
