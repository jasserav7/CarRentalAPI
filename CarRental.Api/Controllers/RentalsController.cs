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
