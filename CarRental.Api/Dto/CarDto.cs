using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Api.Dto
{
    public class CarDto
    {
        public int Id { get; set; } // Requerido para PUT
        [Required]
        public string Brand { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; } // Price per day
        [Required]
        public int Year { get; set; } // Año de fabricación
    }
}