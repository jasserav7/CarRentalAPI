using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;

namespace CarRental.Api.Services
{
    public enum DeleteUserResult
    {
        Deleted,
        NotFound,
        HasRentals,
    }

    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserAsync(int id);
        Task<UserDto> CreateUserAsync(UserDto userDto);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<DeleteUserResult> DeleteUserAsync(int id);
    }
}
