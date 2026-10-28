using System.Threading.Tasks;
using CarRental.Api.Dto;

namespace CarRental.Api.Services
{
    public enum RentalError
    {
        None,
        InvalidPeriod,
        UserNotFound,
        CarNotFound,
        CarNotAvailable,
    }

    public record RentalResult(RentalError Error, string? Message = null, int RentalId = 0, decimal TotalPrice = 0)
    {
        public bool Success => Error == RentalError.None;
    }

    public interface IRentalService
    {
        Task<RentalResult> CreateRentalAsync(RentalDto rentalDto);
    }
}
