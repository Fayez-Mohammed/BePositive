// Base.API/Controllers/Auth/HospitalAuthController.cs

using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers.Auth
{
    [ApiController]
    [Route("api/auth/hospital")]
    public class HospitalAuthController : ControllerBase
    {
        private readonly IHospitalAuthService _hospitalAuthService;

        public HospitalAuthController(IHospitalAuthService hospitalAuthService)
        {
            _hospitalAuthService = hospitalAuthService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] HospitalRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Invalid request.";

                return BadRequest(new ApiErrorResponseDTO
                {
                    StatusCode = 400,
                    Message = errors
                });
            }

            var result = await _hospitalAuthService.RegisterAsync(request);

            return result.StatusCode switch
            {
                201 => StatusCode(201, result),
                409 => Conflict(new ApiErrorResponseDTO { StatusCode = 409, Message = result.Message }),
                400 => BadRequest(new ApiErrorResponseDTO { StatusCode = 400, Message = result.Message }),
                _ => StatusCode(result.StatusCode, result)
            };
        }
    }
}