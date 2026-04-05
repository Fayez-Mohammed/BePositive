// Base.API/Controllers/Hospital/DonorMonitoringController.cs

using Base.API.DTOs;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers.Hospital
{
    [ApiController]
    [Route("api/hospital/donors")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class DonorMonitoringController : ControllerBase
    {
        private readonly IDonorMonitoringService _service;

        public DonorMonitoringController(IDonorMonitoringService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get donor stats — eligible, recently donated, ineligible counts.
        /// Example: GET /api/hospital/donors/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
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
                var result = await _service.GetStatsAsync(userId);
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
        /// Get all donors who responded to this hospital's requests.
        /// Example: GET /api/hospital/donors?status=Eligible&page=1&limit=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDonors(
            [FromQuery] string? search = null,
            [FromQuery] string? bloodTypeId = null,
            [FromQuery] string? status = null,
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
                var result = await _service.GetDonorsAsync(userId,
                    new GetDonorsQuery
                    {
                        Search = search,
                        BloodTypeId = bloodTypeId,
                        Status = status,
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
        /// Get single donor detail for the contact modal.
        /// Example: GET /api/hospital/donors/{donorId}
        /// </summary>
        [HttpGet("{donorId}")]
        public async Task<IActionResult> GetDonor(
            [FromRoute] string donorId)
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
                var result = await _service.GetDonorByIdAsync(userId, donorId);
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
    }
}