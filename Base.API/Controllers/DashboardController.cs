// Base.API/Controllers/Hospital/DashboardController.cs

using Base.API.DTOs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.DashboardDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital/dashboard")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
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

        // ── GET /api/hospital/dashboard/stats ─────────────────────
        /// <summary>
        /// Get dashboard stat cards:
        /// totalDonors, availableBloodUnits, urgentRequests, transfusionsToday.
        /// Each includes a % change vs same period last month.
        /// Example: GET /api/hospital/dashboard/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetStatsAsync(userId);
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

        // ── GET /api/hospital/dashboard/recent-activity ───────────
        /// <summary>
        /// Get last N activity items for the Recent Activity section.
        /// Example: GET /api/hospital/dashboard/recent-activity?limit=4
        /// </summary>
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(
            [FromQuery] int limit = 4)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetRecentActivityAsync(userId, limit);
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

        // ── GET /api/hospital/dashboard/activity-log ──────────────
        /// <summary>
        /// Get full paginated activity log with filters.
        /// Example: GET /api/hospital/dashboard/activity-log?page=1&limit=10
        /// activityType: All, Donation, Request, Inventory
        /// </summary>
        [HttpGet("activity-log")]
        public async Task<IActionResult> GetActivityLog(
            [FromQuery] string?  activityType = null,
            [FromQuery] string?  bloodTypeId  = null,
            [FromQuery] DateOnly? date        = null,
            [FromQuery] int      page         = 1,
            [FromQuery] int      limit        = 10)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetActivityLogAsync(userId,
                    new ActivityLogQuery
                    {
                        ActivityType = activityType,
                        BloodTypeId  = bloodTypeId,
                        Date         = date,
                        Page         = page,
                        Limit        = limit
                    });

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
