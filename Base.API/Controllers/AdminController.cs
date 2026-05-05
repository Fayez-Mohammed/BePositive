
using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Base.Services.Helpers;
using Base.Services.Implementations;
using Base.Services.Interfaces;
using Base.Shared.DTOs;
using Base.Shared.Enums;
using Base.Shared.Responses.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Base.API.Controllers
{
   // [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdmin")] // 🔐 مخصص فقط للمسؤولين
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserProfileService _userProfileService;
        public AdminController(UserManager<ApplicationUser> userManager,IUnitOfWork unitOfWork, IUserProfileService userProfileService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userProfileService = userProfileService; 
        }

        // GET: api/users
        [HttpGet("list")]
        public async Task<ActionResult<UserListDto>> GetAll(
            [FromQuery] string? search,
            [FromQuery] UserTypes? userType,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _userProfileService.GetAllAsync(search, userType, isActive, page, pageSize);
            return Ok(result);
        }

        // GET: api/users/{id}
        [HttpGet("get-user")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var user = await _userProfileService.GetByIdAsync(id);

            if (user == null) return NotFound();
                  if(user.IsDeleted)
                return NotFound("User account has been deleted.");
            return Ok(user);
        }

        // POST: api/users
        [HttpPost("create")]
        public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userProfileService.CreateAsync(request);
            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("update")]
        public async Task<ActionResult<UserDto>> Update(/*string id, */UpdateUserRequest request)
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userProfileService.UpdateAsync(id, request);
            if (user == null) return NotFound();
            if(user.IsDeleted)
                return NotFound("User account has been deleted.");
            return Ok(user);
        }

        // PATCH: api/users/{id}/toggle-active
        [HttpPatch("toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var success = await _userProfileService.ToggleActiveAsync(id);
            if (!success) return Forbid();
            return Ok();
        }

        // DELETE: api/users/{id}
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var resultMessage = await _userProfileService.DeleteAsync(id);
           
            return Ok(resultMessage);
        }

        //[HttpPatch("change-password")]
        //public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto model)
        //{
        //    if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
        //    if (string.IsNullOrEmpty(model.NewPassword)) throw new ArgumentNullException(nameof(model.NewPassword));
        //    if (model.NewPassword.Length < 6)
        //        return BadRequest("Password must be at least 6 characters.");

        //    var success = await _userProfileService.ChangePasswordAsync(id, model.NewPassword);
        //    if (!success) return Forbid();
        //    return Ok();
        //}
        [HttpPatch("change-password")]
    
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.OldPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest("Old and new password are required.");

            if (model.NewPassword.Length < 6)
                return BadRequest("New password must be at least 6 characters.");

            var success = await _userProfileService
                .ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (!success)
                return BadRequest("Old password is incorrect.");

            return Ok("Password changed successfully.");
        }
        ///////////////////////////////
        /// <summary>
        /// Get All Users
        /// </summary>
        /// <remarks>
        /// هذا الـ endpoint يعرض جميع المستخدمين الموجودين في قاعدة البيانات.
        /// يمكن فلترة النتائج حسب الحاجة.
        /// </remarks>
        /// <returns>قائمة المستخدمين</returns>
        /// <response code="200">All Users</response>
        /// <response code="401">you are not authorized</response>
        /// <response code="403">Forbidden to access this end point</response>
        /// <response code="404">No users found in the system.</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("users")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            // 1. جلب جميع المستخدمين إلى الذاكرة.
            // يفضل استخدام ToListAsync() إذا كان متاحاً لجعلها عملية IO حقيقية.
            var users = _userManager.Users.Where(u=>u.IsDeleted==false).ToList();

            if (users == null || !users.Any())
            {
                throw new NotFoundException("No users found in the system.");
            }

            // 2. 🛡️ تحسين الكفاءة: جلب جميع الأدوار الخاصة بجميع المستخدمين دفعة واحدة.
            //    نستخدم .Select() ثم .ToDictionary() لتحويل القائمة إلى قاموس (Dictionary) 
            //    للتأكد من أن عملية البحث عن الأدوار ستكون سريعة جداً (O(1)).
            var userRolesMap = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                // 💡 لا توجد مشكلة تزامن هنا، حيث يتم تنفيذ استعلام GetRolesAsync بشكل متتالٍ
                //    (وهو أفضل من التوازي الذي يسبب خطأ DbContext).
                var roles = await _userManager.GetRolesAsync(user);
                userRolesMap.Add(user.Id, roles);
            }

            // 3. بناء النتيجة في الذاكرة (سريعة جداً)
            var result = users.Select(user => new
            {
                user.Id,
                user.Email,
                // 💡 استخدام القاموس للوصول الفوري للأدوار.
                Roles = userRolesMap.ContainsKey(user.Id) ? userRolesMap[user.Id] : new List<string>()
            }).ToList();

            return Ok(result);
          //  return Ok(new ApiResponseDTO(200, "All Users",result) );
        }

        // <summary>
        /// ✅ Get the current user's info using the JWT token (no need for email)
        /// </summary>
        [HttpGet("GetCurrentUser")]
        [Authorize] // Requires valid JWT token
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var profileRepository = _unitOfWork.Repository<UserProfile>();
                // 1. التحقق الوقائي من حالة التوثيق.
                //    (مع أن [Authorize] يغطي هذا، لكن التأكد يضيف طبقة وضوح).
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    throw new UnauthorizedException("The user is not authenticated.");
                }

                // 2. استخدام GetUserAsync لجلب المستخدم من الـ Claims في الـ HttpContext.
                //    هذا هو الأسلوب الأكثر كفاءة وموصى به في ASP.NET Identity.
                var user = await _userManager.GetUserAsync(User);
                var userDto = user?.ToUserDto();

                if (userDto == null)
                {
                    // قد يحدث إذا تم حذف المستخدم من قاعدة البيانات بعد إصدار التوكن.
                    // (يفضل تسجيل هذه الحالة).
                    // _logger.LogWarning("GetCurrentUser: User not found for authenticated token.");
                    throw new NotFoundException("User account no longer exists in the system.");
                }
                if(userDto.IsDeleted)
                    {
                    // قد يحدث إذا تم حذف المستخدم من قاعدة البيانات بعد إصدار التوكن.
                    // (يفضل تسجيل هذه الحالة).
                    // _logger.LogWarning("GetCurrentUser: User account is marked as deleted for authenticated token.");
                    throw new NotFoundException("User account has been deleted.");
                }
                // 3. جلب الـ Roles والـ Profile (بقاء نفس منطق جلب البيانات).
               // var roles = await _userManager.GetRolesAsync(user);
                
                // جلب الـ Profile مع معالجة احتمال عدم وجوده.
             //   var profile = await profileRepository.GetByIdAsync(userDto.Id);

                // 4. بناء الرد مع إزالة البيانات الحساسة غير الضرورية.
                //var result = new
                //{
                //    user.Id,
                //    user.Email,
                //    user.UserName,
                //    // التأكد من أن حقل الإيميل موجود للمستخدم
                //    IsEmailConfirmed = user.EmailConfirmed,
                //    Roles = roles,
                //    Profile = new
                //    {
                //        // استخدام Null-Conditional Operator (?.) لمعالجة حالة profile == null
                //        //FullName = profile?.FullName,
                //        //PhoneNumber = profile?.PhoneNumber,
                //    }
                //};
                return Ok(new ApiResponseDTO(200, "Current User", userDto));
            }
            catch (Exception ex)
            {
                // 5. تسجيل الخطأ غير المتوقع وإرجاع 500.
                // يفضل دائمًا تسجيل الأخطاء (ex) كاملة في نظام التسجيل (Logging System).
                //_logger.LogError(ex, "GetCurrentUser: An unexpected error occurred while fetching user data.");
                throw new InternalServerException("Internal server error while processing request.");
            }
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUserByUserID(string userID)
        {
            if (string.IsNullOrWhiteSpace(userID))
                throw new BadRequestException("User ID is required.");

            var resultMessage = await _userProfileService.DeleteAsync(userID);
            return Ok(resultMessage);

        }
    }
}
