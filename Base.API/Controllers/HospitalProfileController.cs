// Base.API/Controllers/Hospital/HospitalProfileController.cs

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
    [Route("api/hospital/profile")]
    [Authorize(Roles = nameof(UserTypes.HospitalAdmin))]
    public class HospitalProfileController : ControllerBase
    {
        private readonly IHospitalProfileService _service;

        public HospitalProfileController(IHospitalProfileService service)
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

        /// <summary>
        /// Get the current hospital's profile.
        /// Example: GET /api/hospital/profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.GetProfileAsync(userId);

                if (!result.Success)
                    return NotFound(new ApiErrorResponseDTO
                    {
                        StatusCode = 404,
                        Message    = result.Message
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

        /// <summary>
        /// Update the current hospital's profile.
        /// LicenseNumber is NOT updatable.
        /// Example: PATCH /api/hospital/profile
        /// Body: { name, address, phone, email, cityId, latitude, longitude }
        /// </summary>
        [HttpPatch]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateHospitalProfileDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = "Invalid request data."
                });

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized401();

            try
            {
                var result = await _service.UpdateProfileAsync(userId, dto);

                if (!result.Success)
                    return NotFound(new ApiErrorResponseDTO
                    {
                        StatusCode = 404,
                        Message    = result.Message
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
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message    = ex.Message
                });
            }
        }
    }
}
