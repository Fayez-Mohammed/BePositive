// Base.API/Controllers/HospitalRequestsController.cs
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [ApiController]
    [Route("api/hospital/donors")]
    [Authorize(Roles = "HospitalAdmin")] // تأكد أن المسمى مطابق للـ Enum أو الـ Roles عندك
    public class HospitalDonorsController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public HospitalDonorsController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [HttpGet("eligible-count")]
        public async Task<IActionResult> GetEligibleDonorsCount([FromQuery] GetEligibleDonorsCountDTO dto)
        {
            // سحب الـ NameIdentifier (UserId) من الـ Token الخاص بالأدمن الحالي
            var hospitalAdminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(hospitalAdminUserId))
                return Unauthorized();

            var result = await _hospitalService.GetNearbyEligibleDonorsCountAsync(hospitalAdminUserId, dto);

            // الـ Middleware الخاص بك سيقوم بتغليف النتيجة تلقائيًا بناءً على الـ Global Structure لمشروعك
            return Ok(result);
        }
    }
}