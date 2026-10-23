using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().Property(c => c.Price).HasPrecision(18, 2); // Configura la precisión del campo Price a 18 dígitos con 2 decimales
            modelBuilder.Entity<Rental>().Property(r => r.Price).HasPrecision(18, 2);
            base.OnModelCreating(modelBuilder);
        }
    }
}
