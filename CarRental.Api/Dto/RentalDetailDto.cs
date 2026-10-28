using System;

namespace CarRental.Api.Dto
{
    public class RentalDetailDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public int CarId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
