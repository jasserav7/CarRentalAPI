using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;

namespace CarRental.Api.Services
{
    public interface IReportService
    {
        Task<SummaryReportDto> GetSummaryAsync();
        Task<List<RevenueByCarDto>> GetRevenueByCarAsync();
    }
}
