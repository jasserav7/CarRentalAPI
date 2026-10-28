using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Api.Dto;
using CarRental.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/reports/summary
        [HttpGet("summary")]
        public async Task<ActionResult<SummaryReportDto>> GetSummary()
        {
            return await _reportService.GetSummaryAsync();
        }

        // GET: api/reports/revenue-by-car
        [HttpGet("revenue-by-car")]
        public async Task<ActionResult<IEnumerable<RevenueByCarDto>>> GetRevenueByCar()
        {
            var report = await _reportService.GetRevenueByCarAsync();
            return report;
        }
    }
}
