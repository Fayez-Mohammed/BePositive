// Base.API/Controllers/Admin/AdminBloodRequestController.cs

using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.DTOs.AdminDTOs;
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

        ///////////new 
        ///
    



        // ── GET /api/admin/requests/hospital/{hospitalId} ─────
        /// <summary>
        /// Get all requests for a specific hospital.
        /// Useful for drilling into a hospital's request history.
        /// Filters: status, urgencyLevel
        /// </summary>
        [HttpGet("hospital/{hospitalId}")]
        public async Task<IActionResult> GetRequestsByHospital(
            string hospitalId,
            [FromQuery] string? status = null,
            [FromQuery] string? urgencyLevel = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _service.GetRequestsByHospitalAsync(
                    hospitalId, status, urgencyLevel, page, limit);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
        }

       

        // ── PATCH /api/admin/requests/{id}/status ─────────────
        /// <summary>
        /// Update a request's status. Admin can only set Cancelled or Expired.
        /// Fulfilled status is set automatically by the system.
        /// Body: { "status": "Cancelled" | "Expired", "note": "optional reason" }
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            string id,
            [FromBody] UpdateRequestStatusDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = "Invalid request data."
                });

            try
            {
                var result = await _service.UpdateRequestStatusAsync(id, dto);

                if (!result.Success)
                    return NotFound(new ApiErrorResponseDTO
                    {
                        StatusCode = 404,
                        Message = result.Message
                    });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        // ── DELETE /api/admin/requests/{id} ───────────────────
        /// <summary>
        /// Soft delete a blood request.
        /// The request is marked as deleted and hidden from all views.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(string id)
        {
            try
            {
                var result = await _service.DeleteRequestAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
        }
    }
}
