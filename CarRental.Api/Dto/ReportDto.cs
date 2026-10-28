namespace CarRental.Api.Dto
{
    public class MostRentedCarDto
    {
        public int CarId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int RentalCount { get; set; }
    }

    public class SummaryReportDto
    {
        public int TotalRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public MostRentedCarDto? MostRentedCar { get; set; } // null when there are no rentals
    }

    public class RevenueByCarDto
    {
        public int CarId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int RentalCount { get; set; }
        public int TotalDays { get; set; }
        public decimal Revenue { get; set; }
    }
}
