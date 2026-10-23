using System;
using System.Collections.Generic;

namespace CarRental.Api.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public decimal Price { get; set; } // Price per day
        public int Year { get; set; } // Año de fabricación
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
