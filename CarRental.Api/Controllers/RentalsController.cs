using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;
using CarRental.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // GET: api/rentals?userId=1&carId=2&from=2027-01-01&to=2027-02-01
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalDetailDto>>> GetRentals(
            [FromQuery] int? userId, [FromQuery] int? carId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (from.HasValue && to.HasValue && from >= to)
                return BadRequest("'from' must be before 'to'.");

            var rentals = await _rentalService.GetRentalsAsync(userId, carId, from, to);
            return rentals;
        }

        // GET: api/rentals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RentalDetailDto>> GetRental(int id)
        {
            var rental = await _rentalService.GetRentalAsync(id);
            if (rental == null)
                return NotFound();

            return rental;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] RentalDto rentalDto)
        {
            var result = await _rentalService.CreateRentalAsync(rentalDto);

            return result.Error switch
            {
                RentalError.None => Ok(new { Id = result.RentalId, result.TotalPrice }),
                RentalError.InvalidPeriod => BadRequest(result.Message),
                RentalError.UserNotFound or RentalError.CarNotFound => NotFound(result.Message),
                RentalError.CarNotAvailable => Conflict(result.Message),
                _ => StatusCode(500),
            };
        }
    }
}
