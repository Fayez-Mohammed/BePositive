// Base.API/Controllers/Admin/AdminBloodRequestController.cs

using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/requests")]
    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
    public class AdminBloodRequestController : ControllerBase
    {
        private readonly IAdminBloodRequestService _service;

        public AdminBloodRequestController(IAdminBloodRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get system-wide blood request statistics.
        /// Example: GET /api/admin/requests/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all blood requests across all hospitals with filters.
        /// Example: GET /api/admin/requests?page=1&limit=10&status=Open
        /// Filters: hospitalId, bloodTypeId, status (Open/Fulfilled/Cancelled/Expired),
        ///          urgencyLevel (Routine/Urgent/Critical), search
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRequests(
            [FromQuery] string? hospitalId = null,
            [FromQuery] string? bloodTypeId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? urgencyLevel = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var result = await _service.GetAllRequestsAsync(
                hospitalId, bloodTypeId, status, urgencyLevel,
                search, page, limit);

            return Ok(result);
        }

        /// <summary>
        /// Get blood request detail by ID with full hospital and donor info.
        /// Example: GET /api/admin/requests/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(
            [FromRoute] string id)
        {
            var result = await _service.GetRequestByIdAsync(id);

            if (!result.Success)
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = result.Message
                });

            return Ok(result);
        }
        /// <summary>
        /// Retrieves a list of all hospitals.
        /// </summary>
        /// <remarks>This endpoint is accessible via HTTP GET at 'hospitals'. The response is wrapped
        /// according to the application's response middleware.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the collection of hospitals. The response includes an empty
        /// collection if no hospitals are found.</returns>
        [HttpGet("hospitals")]
        public async Task<IActionResult> GetHospitals()
        {
            var result = await _service.GetHospitalsAsync();
            if (!result.Success)
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = result.Message
                });
            return Ok(result);
        }
    }
}
