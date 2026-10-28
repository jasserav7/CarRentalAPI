using CarRental.Api.Data;
using CarRental.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Tests
{
    /// <summary>
    /// In-memory SQLite database with the real schema. SQLite (unlike the EF InMemory provider)
    /// enforces foreign keys, so cascade behavior matches what SQL Server does.
    /// </summary>
    public sealed class TestDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public AppDbContext CreateContext() => new(_options);

        public User AddUser(string username = "john", string email = "john@example.com")
        {
            using var context = CreateContext();
            var user = new User { Username = username, Email = email };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public Car AddCar(string brand = "Toyota", string model = "Corolla", decimal price = 50m, int? year = null)
        {
            using var context = CreateContext();
            var car = new Car { Brand = brand, Model = model, Price = price, Year = year ?? DateTime.Now.Year };
            context.Cars.Add(car);
            context.SaveChanges();
            return car;
        }

        public Rental AddRental(int userId, int carId, DateTime start, DateTime end, decimal price)
        {
            using var context = CreateContext();
            var rental = new Rental { UserId = userId, CarId = carId, StartDate = start, EndDate = end, Price = price };
            context.Rentals.Add(rental);
            context.SaveChanges();
            return rental;
        }

        public void Dispose() => _connection.Dispose();
    }
}
