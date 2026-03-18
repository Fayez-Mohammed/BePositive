// Base.API/Controllers/Hospital/BloodRequestController.cs

using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital/requests")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class BloodRequestController : ControllerBase
    {
        private readonly IBloodRequestService _service;

        public BloodRequestController(IBloodRequestService service)
        {
            _service = service;
        }
       
        /// <summary>
        /// Create a new blood donation request.
        /// Called when hospital admin submits the Create New Blood Request form.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRequest(
            [FromBody] CreateBloodRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = "Invalid request data."
                });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = "Unauthorized."
                });

            try
            {
                var result = await _service.CreateRequestAsync(userId, dto);

                return StatusCode(201, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
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
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        // Add these actions inside BloodRequestController class

        /// <summary>
        /// Get all blood requests for this hospital with optional filters.
        /// Example: GET /api/hospital/requests?status=Open&page=1&limit=10
        /// </summary>
        /// <summary>
        /// Get all blood requests for this hospital with optional filters.
        /// Example: GET /api/hospital/requests?status=Open&page=1&limit=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRequests(
            [FromQuery] string? search = null,
            [FromQuery] RequestStatus? status = null,
            [FromQuery] UrgencyLevel? urgencyLevel = null,
            [FromQuery] string? bloodTypeId = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = "Unauthorized."
                });

            try
            {
                var result = await _service.GetAllRequestsAsync(userId,
                    new GetBloodRequestsQuery
                    {
                        Search = search,
                        Status = status,
                        UrgencyLevel = urgencyLevel,
                        BloodTypeId = bloodTypeId,
                        Page = page,
                        Limit = limit
                    });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = ex.Message
                });
            }
        }
        /// <summary>
        /// Get single request details by id.
        /// Example: GET /api/hospital/requests/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(
            [FromRoute] string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = "Unauthorized."
                });

            try
            {
                var result = await _service.GetRequestByIdAsync(userId, id);
                if (!result.Success)
                    return NotFound(new ApiErrorResponseDTO
                    {
                        StatusCode = 404,
                        Message = result.Message
                    });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing blood request (urgency, quantity, note, deadline).
        /// Example: PATCH /api/hospital/requests/{id}
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateRequest(
            [FromRoute] string id,
            [FromBody] UpdateBloodRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = "Unauthorized."
                });

            try
            {
                var result = await _service.UpdateRequestAsync(userId, id, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
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

        /// <summary>
        /// Cancel an open blood request.
        /// Example: PATCH /api/hospital/requests/{id}/cancel
        /// </summary>
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(
            [FromRoute] string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = "Unauthorized."
                });

            try
            {
                await _service.CancelRequestAsync(userId, id);
                return Ok(new { success = true, message = "Request cancelled successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponseDTO
                {
                    StatusCode = 401,
                    Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponseDTO
                {
                    StatusCode = 404,
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

    }
}