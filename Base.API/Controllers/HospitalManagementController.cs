// Base.API/Controllers/Admin/HospitalManagementController.cs

using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/hospitals")]
    [Authorize(Roles = nameof(UserTypes.SystemAdmin))]
    public class HospitalManagementController : ControllerBase
    {
        private readonly IHospitalManagementService _service;

        public HospitalManagementController(IHospitalManagementService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all hospitals with optional status filter and pagination.
        /// Example: GET /api/admin/hospitals?status=Active&page=1&limit=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllHospitals(
            [FromQuery] HospitalStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var result = await _service.GetAllHospitalsAsync(status, page, limit);
            return Ok(result);
        }

        /// <summary>
        /// Get hospitals statistics (total, active, suspended, underreview).
        /// Example: GET /api/admin/hospitals/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get hospital details by id.
        /// Example: GET /api/admin/hospitals/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHospital(
            [FromRoute] string id)
        {
            var result = await _service.GetHospitalByIdAsync(id);
            if (!result.Success)
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = result.Message
                });

            return Ok(result);
        }

        /// <summary>
        /// Update hospital status to any value (UnderReview, Active, Suspended).
        /// Example: PATCH /api/admin/hospitals/{id}/status
        /// Body: { "status": "Active" }
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            [FromRoute] string id,
            [FromBody] UpdateHospitalStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateStatusAsync(id, request.Status);
            if (!result.Success)
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = result.Message
                });

            return Ok(result);
        }

        /// <summary>
        /// Activate a hospital account directly.
        /// Example: PATCH /api/admin/hospitals/{id}/activate
        /// </summary>
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(
            [FromRoute] string id)
        {
            var result = await _service.ActivateAsync(id);
            if (!result.Success)
                return result.Message.Contains("not found")
                    ? NotFound(new ApiErrorResponseDTO { StatusCode = 404, Message = result.Message })
                    : BadRequest(new ApiErrorResponseDTO { StatusCode = 400, Message = result.Message });

            return Ok(result);
        }

        /// <summary>
        /// Suspend a hospital account.
        /// Example: PATCH /api/admin/hospitals/{id}/suspend
        /// </summary>
        [HttpPatch("{id}/suspend")]
        public async Task<IActionResult> Suspend(
            [FromRoute] string id)
        {
            var result = await _service.SuspendAsync(id);
            if (!result.Success)
                return result.Message.Contains("not found")
                    ? NotFound(new ApiErrorResponseDTO { StatusCode = 404, Message = result.Message })
                    : BadRequest(new ApiErrorResponseDTO { StatusCode = 400, Message = result.Message });

            return Ok(result);
        }

        /// <summary>
        /// Soft delete a hospital by id.
        /// Example: DELETE /api/admin/hospitals/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            [FromRoute] string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _service.DeleteAsync(id, currentUserId);
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