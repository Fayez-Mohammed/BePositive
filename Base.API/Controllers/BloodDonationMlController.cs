// Base.API.Controllers/AdminControllers/BloodDonationMlController.cs
using Base.Services.Implementations;
using Base.Services.Interfaces;
using Base.Shared.DTOs.MlDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers.AdminControllers
{
    [Authorize] // مأمن للسيستم بالكامل
    [ApiController]
    [Route("api/ml")]
    public class BloodDonationMlController : ControllerBase
    {
        private readonly IMlPredictionService _mlService;

        public BloodDonationMlController(IMlPredictionService mlService)
        {
            _mlService = mlService;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> PredictDonors([FromBody] MlPredictionQueryDTO query)
        {
            var result = await _mlService.GetDonorsPredictionsAsync(query.DonorId);

            if (!result.Any())
                return NotFound("No eligible donors found for prediction.");

            return Ok(new
            {
                Success = true,
                Message = string.IsNullOrWhiteSpace(query.DonorId)
                    ? "Predictions generated successfully for all eligible donors."
                    : $"Prediction generated successfully for donor ID: {query.DonorId}",
                Data = result
            });
        }

        // ضيف الـ Endpoint دي جوه كلاس BloodDonationMlController

        [HttpGet("health")]
        public async Task<IActionResult> CheckMlServiceHealth()
        {
            var healthResult = await _mlService.CheckMlHealthAsync();

            if (healthResult == null || healthResult.Status != "ok" || !healthResult.ModelLoaded)
            {
                // لو السيرفر نايم أو الموديل مش متقري هيرجع 503 Service Unavailable
                return StatusCode(503, new
                {
                    Success = false,
                    Message = "ML Service is currently unavailable or model is spinning up on Render. Please try again in a few seconds."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "ML Service is UP and CatBoost model is loaded successfully.",
                Details = healthResult
            });
        }
    }
}