using Base.Services.Interfaces;
using Base.Shared.DTOs.DonorDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [ApiController]
    [Route("api/donor/requests")]
   // [Authorize(Roles = "Donor")] // حماية تامة برتبة المتبرعين
    public class DonorRequestsController : ControllerBase
    {
        private readonly IDonorService _donorService;

        public DonorRequestsController(IDonorService donorService)
        {
            _donorService = donorService;
        }
        // 1. Endpoint إكمال البيانات وترقية الحساب
        [HttpPost("complete")]
        [Authorize] // أي مستخدم مسجل دخول يقدر يكمل بياناته ليكون متبرع
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _donorService.CompleteDonorProfileAsync(userId, dto);
                return Ok(new
                {
                    statusCode = 200,
                    success = true,
                    message = "Profile completed successfully. You are now registered as an active donor!"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        // 2. Endpoint تحديث الـ FCM Token تلقائياً في الخلفية
        [HttpPut("update-fcm")]
        [Authorize(Roles = "Donor")] // مخصصة للمتبرعين فقط لتحديث أجهزتهم
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isSuccess = await _donorService.UpdateFcmTokenAsync(userId, dto);

            if (!isSuccess)
                return BadRequest(new { message = "User is not registered as a donor." });

            return Ok(new { success = true, message = "FCM Token updated successfully." });
        }
        [HttpPost("accept")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> AcceptRequest([FromBody] AcceptRequestDTO dto)
        {
            // سحب الـ UserId برمجياً من الـ JWT Token بكل أمان
            var donorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(donorUserId))
                return Unauthorized(new { message = "Invalid or expired token claims." });

            try
            {
                // استدعاء الخدمة المحدثة والحصول على إحداثيات الـ GPS للمستشفى
                var navigationResult = await _donorService.AcceptBloodRequestAsync(donorUserId, dto);

                return Ok(new
                {
                    success = true,
                    message = "Request accepted successfully.",
                    navigation = navigationResult
                });
            }
            catch (InvalidOperationException ex)
            {
                // معالجة الأخطاء والقيود الطبية وحالة الطلب
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Unauthorized claim.");
        [HttpGet("nearby")]
        [Authorize(Roles = "Donor")] // مأمن تـماماً برتبة المتبرع
        public async Task<IActionResult> GetNearbyRequests([FromQuery] NearbyRequestQueryDTO query)
        {

            var result = await _donorService.GetNearbyRequestsAsync(UserId, query);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}