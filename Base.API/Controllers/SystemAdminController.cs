// ════════════════════════════════════════════════════════════
// Base.API/Controllers/Admin/AdminDashboardController.cs
// ════════════════════════════════════════════════════════════
using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _service;

        public AdminDashboardController(IAdminDashboardService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get system-wide dashboard stats.
        /// Example: GET /api/admin/dashboard/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get last N hospital registrations.
        /// Example: GET /api/admin/dashboard/recent-registrations?limit=5
        /// </summary>
        [HttpGet("recent-registrations")]
        public async Task<IActionResult> GetRecentRegistrations(
            [FromQuery] int limit = 5)
        {
            var result = await _service.GetRecentRegistrationsAsync(limit);
            return Ok(result);
        }

        /// <summary>
        /// Get activity chart data — last 12 months.
        /// Example: GET /api/admin/dashboard/activity-chart
        /// </summary>
        [HttpGet("activity-chart")]
        public async Task<IActionResult> GetActivityChart()
        {
            var result = await _service.GetActivityChartAsync();
            return Ok(result);
        }
    }
}


// ════════════════════════════════════════════════════════════
// Base.API/Controllers/Admin/AdminDonorController.cs
// ════════════════════════════════════════════════════════════

namespace Base.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/donors")]
    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
    public class AdminDonorController : ControllerBase
    {
        private readonly IAdminDonorService _service;

        public AdminDonorController(IAdminDonorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get system-wide donor stats.
        /// Example: GET /api/admin/donors/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all donors with optional filters.
        /// Example: GET /api/admin/donors?page=1&limit=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllDonors(
            [FromQuery] string? search      = null,
            [FromQuery] string? bloodTypeId = null,
            [FromQuery] string? status      = null,
            [FromQuery] int     page        = 1,
            [FromQuery] int     limit       = 10)
        {
            var result = await _service.GetAllDonorsAsync(
                search, bloodTypeId, status, page, limit);
            return Ok(result);
        }

        /// <summary>
        /// Get donor detail by id.
        /// Example: GET /api/admin/donors/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDonor(
            [FromRoute] string id)
        {
            var result = await _service.GetDonorByIdAsync(id);
            if (!result.Success)
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message    = result.Message
                });

            return Ok(result);
        }
    }
}


//// ════════════════════════════════════════════════════════════
//// Base.API/Controllers/Admin/AdminUserController.cs
//// ════════════════════════════════════════════════════════════

////using Base.Shared.DTOs.SystemAdminDTOs;

//namespace Base.API.Controllers.Admin
//{
//    [ApiController]
//    [Route("api/admin/users")]
//    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
//    public class AdminUserController : ControllerBase
//    {
//        private readonly IAdminUserService _service;

//        public AdminUserController(IAdminUserService service)
//        {
//            _service = service;
//        }

//        /////// <summary>
//        /////// Get all users with optional filters.
//        /////// Example: GET /api/admin/users?page=1&limit=10
//        /////// userType: Donor, HospitalAdmin, SystemAdmin
//        /////// </summary>
//        ////[HttpGet]
//        ////public async Task<IActionResult> GetAllUsersSA(
//        ////    [FromQuery] string? search   = null,
//        ////    [FromQuery] string? userType = null,
//        ////    [FromQuery] int     page     = 1,
//        ////    [FromQuery] int     limit    = 10)
//        ////{
//        ////    var result = await _service.GetAllUsersAsync(
//        ////        search, userType, page, limit);
//        ////    return Ok(result);
//        ////}

//        ///// <summary>
//        ///// Get user detail by id.
//        ///// Example: GET /api/admin/users/{id}
//        ///// </summary>
//        //[HttpGet("{id}")]
//        //public async Task<IActionResult> GetUserSA(
//        //    [FromRoute] string id)
//        //{
//        //    var result = await _service.GetUserByIdAsync(id);
//        //    if (!result.Success)
//        //        return NotFound(new ApiErrorResponseDTO
//        //        {
//        //            StatusCode = 404,
//        //            Message    = result.Message
//        //        });

//        //    return Ok(result);
//        //}

//        ///// <summary>
//        ///// Activate or deactivate a user account.
//        ///// Example: PATCH /api/admin/users/{id}/status
//        ///// Body: { "isActive": true }
//        ///// </summary>
//        //[HttpPatch("{id}/status")]
//        //public async Task<IActionResult> UpdateStatusSA(
//        //    [FromRoute] string            id,
//        //    [FromBody]  UpdateUserStatusDTO dto)
//        //{
//        //    try
//        //    {
//        //        await _service.UpdateUserStatusAsync(id, dto.IsActive);
//        //        return Ok(new
//        //        {
//        //            success = true,
//        //            message = dto.IsActive
//        //                ? "User activated successfully."
//        //                : "User deactivated successfully."
//        //        });
//        //    }
//        //    catch (KeyNotFoundException ex)
//        //    {
//        //        return NotFound(new ApiErrorResponseDTO
//        //        {
//        //            StatusCode = 404,
//        //            Message    = ex.Message
//        //        });
//        //    }
//        //}
//    }
//}


// ════════════════════════════════════════════════════════════
// Base.API/Controllers/Admin/AdminAnalyticsController.cs
// ════════════════════════════════════════════════════════════

namespace Base.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IAdminAnalyticsService _service;

        public AdminAnalyticsController(IAdminAnalyticsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get system-wide analytics summary.
        /// Example: GET /api/admin/analytics/summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get donations vs requests trend — last 12 months.
        /// Example: GET /api/admin/analytics/donations-trend
        /// </summary>
        [HttpGet("donations-trend")]
        public async Task<IActionResult> GetDonationsTrend()
        {
            var result = await _service.GetDonationsTrendAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get hospital count by governorate.
        /// Example: GET /api/admin/analytics/hospitals-by-governorate
        /// </summary>
        [HttpGet("hospitals-by-governorate")]
        public async Task<IActionResult> GetHospitalsByGovernorate()
        {
            var result = await _service.GetHospitalsByGovernorateAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get blood type demand — most requested types system-wide.
        /// Example: GET /api/admin/analytics/blood-type-demand
        /// </summary>
        [HttpGet("blood-type-demand")]
        public async Task<IActionResult> GetBloodTypeDemand()
        {
            var result = await _service.GetBloodTypeDemandAsync();
            return Ok(result);
        }
    }
}
