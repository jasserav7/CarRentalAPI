using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Data;
using CarRental.Api.Dto;
using CarRental.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        private static UserDto ToDto(User user) =>
            new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
            };

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(ToDto).ToList();
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : ToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ToDto(user);
        }

        public async Task<bool> UpdateUserAsync(int id, UserDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Username = userDto.Username;
            user.Email = userDto.Email;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DeleteUserResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return DeleteUserResult.NotFound;

            // The Rental -> User FK cascades, so block the delete to avoid silently losing rental history
            if (await _context.Rentals.AnyAsync(r => r.UserId == id))
                return DeleteUserResult.HasRentals;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return DeleteUserResult.Deleted;
        }
    }
}
