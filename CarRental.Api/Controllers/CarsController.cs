using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Data;
using CarRental.Api.Dto;
using CarRental.Api.Models;
using CarRental.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        private bool IsValidYear(int year)
        {
            int currentYear = DateTime.Now.Year;
            return year >= currentYear - 5;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetCars()
        {
            var cars = await _carService.GetCarsAsync();
            return cars;
            //return NoContent();
        }

        // GET: api/cars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> GetCar(int id)
        {
            var car = await _carService.GetCarAsync(id);
            if (car == null)
                return NotFound();
            return car;
        }

        // POST: api/cars
        [HttpPost]
        public async Task<ActionResult<CarDto>> CreateCar(CarDto carDto)
        {
            if (!IsValidYear(carDto.Year))
                return BadRequest("The car cannot be older than 5 years.");

            var createdCar = await _carService.CreateCarAsync(carDto);
            return CreatedAtAction(nameof(GetCar), new { id = createdCar.Id }, createdCar);
        }

        // PUT: api/cars/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, CarDto carDto)
        {
            if (id != carDto.Id)
                return BadRequest();

            if (!IsValidYear(carDto.Year))
                return BadRequest("The car cannot be older than 5 years.");

            var updated = await _carService.UpdateCarAsync(id, carDto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/cars/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var deleted = await _carService.DeleteCarAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

    }
}