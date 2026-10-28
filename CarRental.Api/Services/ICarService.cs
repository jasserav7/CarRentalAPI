using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;

namespace CarRental.Api.Services
{
    public interface ICarService
    {
        Task<List<CarDto>> GetCarsAsync();
        Task<CarDto?> GetCarAsync(int id);
        Task<CarDto> CreateCarAsync(CarDto carDto);
        Task<bool> UpdateCarAsync(int id, CarDto carDto);
        Task<bool> DeleteCarAsync(int id);
    }
}
