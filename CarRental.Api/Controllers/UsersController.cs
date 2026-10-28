using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;
using CarRental.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRentalService _rentalService;

        public UsersController(IUserService userService, IRentalService rentalService)
        {
            _userService = userService;
            _rentalService = rentalService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return users;
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null)
                return NotFound();

            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserDto userDto)
        {
            var createdUser = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        // GET: api/users/5/rentals
        [HttpGet("{id}/rentals")]
        public async Task<ActionResult<IEnumerable<RentalDetailDto>>> GetUserRentals(int id)
        {
            if (await _userService.GetUserAsync(id) == null)
                return NotFound();

            var rentals = await _rentalService.GetRentalsAsync(id, null, null, null);
            return rentals;
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest();

            var updated = await _userService.UpdateUserAsync(id, userDto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);

            return result switch
            {
                DeleteUserResult.Deleted => NoContent(),
                DeleteUserResult.NotFound => NotFound(),
                DeleteUserResult.HasRentals => Conflict("The user has rentals and cannot be deleted."),
                _ => StatusCode(500),
            };
        }
    }
}
