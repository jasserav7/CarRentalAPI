using CarRental.Api.Dto;
using CarRental.Api.Services;

namespace CarRental.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly TestDb _db = new();

        public void Dispose() => _db.Dispose();

        private UserService CreateService() => new(_db.CreateContext());

        [Fact]
        public async Task CreateUser_PersistsAndReturnsGeneratedId()
        {
            var created = await CreateService().CreateUserAsync(new UserDto { Username = "ana", Email = "ana@example.com" });

            Assert.True(created.Id > 0);
            var fetched = await CreateService().GetUserAsync(created.Id);
            Assert.Equal("ana", fetched!.Username);
        }

        [Fact]
        public async Task GetUser_ReturnsNull_WhenMissing()
        {
            Assert.Null(await CreateService().GetUserAsync(999));
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            _db.AddUser("a", "a@example.com");
            _db.AddUser("b", "b@example.com");

            var users = await CreateService().GetUsersAsync();

            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task UpdateUser_ChangesFields()
        {
            var user = _db.AddUser("old", "old@example.com");

            var updated = await CreateService().UpdateUserAsync(user.Id, new UserDto { Id = user.Id, Username = "new", Email = "new@example.com" });

            Assert.True(updated);
            var fetched = await CreateService().GetUserAsync(user.Id);
            Assert.Equal("new", fetched!.Username);
            Assert.Equal("new@example.com", fetched.Email);
        }

        [Fact]
        public async Task UpdateUser_ReturnsFalse_WhenMissing()
        {
            Assert.False(await CreateService().UpdateUserAsync(999, new UserDto { Username = "x", Email = "x@example.com" }));
        }

        [Fact]
        public async Task DeleteUser_RemovesUser()
        {
            var user = _db.AddUser();

            var result = await CreateService().DeleteUserAsync(user.Id);

            Assert.Equal(DeleteUserResult.Deleted, result);
            Assert.Null(await CreateService().GetUserAsync(user.Id));
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenMissing()
        {
            Assert.Equal(DeleteUserResult.NotFound, await CreateService().DeleteUserAsync(999));
        }

        [Fact]
        public async Task DeleteUser_IsBlocked_WhenUserHasRentals_AndKeepsTheRentals()
        {
            var user = _db.AddUser();
            var car = _db.AddCar();
            _db.AddRental(user.Id, car.Id, new DateTime(2027, 1, 10), new DateTime(2027, 1, 12), 100m);

            var result = await CreateService().DeleteUserAsync(user.Id);

            Assert.Equal(DeleteUserResult.HasRentals, result);
            Assert.NotNull(await CreateService().GetUserAsync(user.Id));
            using var context = _db.CreateContext();
            Assert.Equal(1, context.Rentals.Count());
        }
    }
}
