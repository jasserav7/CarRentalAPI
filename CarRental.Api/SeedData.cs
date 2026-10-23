using CarRental.Api.Data;
using CarRental.Api.Models;

namespace CarRental.Api
{
    public static class SeedData
    {
        public static void Initialize(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Username = "John Doe", Email = "john.doe@example.com" },
                    new User { Username = "Jane Smith", Email = "jane.smith@example.com" }
                );
            }

            if (!context.Cars.Any())
            {
                var year = DateTime.Now.Year;
                context.Cars.AddRange(
                    new Car { Brand = "Toyota", Model = "Corolla", Price = 45.00m, Year = year - 1 },
                    new Car { Brand = "Mazda", Model = "3", Price = 50.00m, Year = year - 2 },
                    new Car { Brand = "Mercedes-Benz", Model = "E 320", Price = 90.00m, Year = year - 3 }
                );
            }

            context.SaveChanges();
        }
    }
}
