using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Dto
{
    public class RentalDto
    {
        [Required]
        public int UserId { get; set; } // ID del usuario que realiza el alquiler

        [Required]
        public int CarId { get; set; } // ID del coche que se va a alquilar

        [Required]
        public DateTime StartDate { get; set; } // Fecha de inicio del alquiler

        [Required]
        public DateTime EndDate { get; set; } // Fecha de fin del alquiler
    }
}
