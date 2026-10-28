using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Data;
using CarRental.Api.Dto;
using CarRental.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    public class CarService : ICarService
    {
        private readonly AppDbContext _context;

        public CarService(AppDbContext context)
        {
            _context = context;
        }

        private static CarDto ToDto(Car car) =>
            new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Price = car.Price,
                Year = car.Year,
            };

        public async Task<List<CarDto>> GetCarsAsync()
        {
            var cars = await _context.Cars.ToListAsync();
            return cars.Select(ToDto).ToList();
        }

        public async Task<CarDto?> GetCarAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            return car == null ? null : ToDto(car);
        }

        public async Task<CarDto> CreateCarAsync(CarDto carDto)
        {
            var car = new Car
            {
                Brand = carDto.Brand,
                Model = carDto.Model,
                Price = carDto.Price,
                Year = carDto.Year,
            };
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return ToDto(car);
        }

        public async Task<bool> UpdateCarAsync(int id, CarDto carDto)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return false;

            car.Brand = carDto.Brand;
            car.Model = carDto.Model;
            car.Price = carDto.Price;
            car.Year = carDto.Year;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return false;

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
