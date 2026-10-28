using CarRental.Api.Dto;
using CarRental.Api.Services;

namespace CarRental.Tests.Services
{
    public class CarServiceTests : IDisposable
    {
        private readonly TestDb _db = new();

        public void Dispose() => _db.Dispose();

        private CarService CreateService() => new(_db.CreateContext());

        [Fact]
        public async Task CreateCar_PersistsAllFields()
        {
            var created = await CreateService().CreateCarAsync(new CarDto { Brand = "Kia", Model = "Rio", Price = 40.5m, Year = 2025 });

            var fetched = await CreateService().GetCarAsync(created.Id);
            Assert.Equal("Kia", fetched!.Brand);
            Assert.Equal("Rio", fetched.Model);
            Assert.Equal(40.5m, fetched.Price);
            Assert.Equal(2025, fetched.Year);
        }

        [Fact]
        public async Task GetCar_ReturnsNull_WhenMissing()
        {
            Assert.Null(await CreateService().GetCarAsync(999));
        }

        [Fact]
        public async Task UpdateCar_ChangesFields_AndReturnsFalseWhenMissing()
        {
            var car = _db.AddCar("Old", "Model", 10m, 2025);

            var updated = await CreateService().UpdateCarAsync(car.Id, new CarDto { Id = car.Id, Brand = "New", Model = "Model2", Price = 20m, Year = 2026 });

            Assert.True(updated);
            var fetched = await CreateService().GetCarAsync(car.Id);
            Assert.Equal("New", fetched!.Brand);
            Assert.Equal(20m, fetched.Price);
            Assert.False(await CreateService().UpdateCarAsync(999, new CarDto { Brand = "x", Model = "x", Price = 1m, Year = 2026 }));
        }

        [Fact]
        public async Task DeleteCar_RemovesCar_AndReturnsFalseWhenMissing()
        {
            var car = _db.AddCar();

            Assert.True(await CreateService().DeleteCarAsync(car.Id));
            Assert.Null(await CreateService().GetCarAsync(car.Id));
            Assert.False(await CreateService().DeleteCarAsync(car.Id));
        }
    }
}
