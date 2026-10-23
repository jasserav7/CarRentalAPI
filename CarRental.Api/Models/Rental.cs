using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Models
{
    public class Rental
    {

        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; } // Total price of the rental
    }
}
