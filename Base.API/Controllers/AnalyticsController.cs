// Base.API/Controllers/Hospital/AnalyticsController.cs

using Base.API.DTOs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.AnalyticsDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital/analytics")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _service;

        public AnalyticsController(IAnalyticsService service)
        {
            _service = service;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult Unauthorized401() =>
            Unauthorized(new ApiErrorResponseDTO
            {
                StatusCode = 401,
                Message    = "Unauthorized."
            });

        // ── GET /api/hospital/analytics/summary ───────────────────
        /// <summary>
        /// Get analytics summary — 3 stat cards with % change.
        /// Example: GET /api/hospital/analytics/summary?period=Last7Days
        /// period options: Last7Days, Last30Days, Last3Months, LastYear
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AnalyticsPeriod period = AnalyticsPeriod.Last7Days)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetSummaryAsync(userId, period);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/analytics/trends ────────────────────
        /// <summary>
        /// Get donation vs requests trend data for the line chart.
        /// Example: GET /api/hospital/analytics/trends?period=Last7Days
        /// Last7Days   → 7 daily points
        /// Last30Days  → 30 daily points
        /// Last3Months → 12 weekly points
        /// LastYear    → 12 monthly points
        /// </summary>
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends(
            [FromQuery] AnalyticsPeriod period = AnalyticsPeriod.Last7Days)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetTrendsAsync(userId, period);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }

        // ── GET /api/hospital/analytics/blood-type-distribution ───
        /// <summary>
        /// Get blood type distribution for the donut chart.
        /// Example: GET /api/hospital/analytics/blood-type-distribution?period=Last7Days
        /// </summary>
        [HttpGet("blood-type-distribution")]
        public async Task<IActionResult> GetBloodTypeDistribution(
            [FromQuery] AnalyticsPeriod period = AnalyticsPeriod.Last7Days)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service
                    .GetBloodTypeDistributionAsync(userId, period);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message    = ex.Message
                });
            }
        }
    }
}
