using Base.API.DTOs;
using Base.Services.Interfaces;
using Base.Shared.DTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Base.API.Controllers
{
  //  [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "SystemAdmin")]
   // [Authorize(Policy = "ActiveUserOnly")]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UsersController(IUserProfileService userProfileService)
        {
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
        public async Task<ActionResult<UserDto>> Update(string id, UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userProfileService.UpdateAsync(id, request);
            if (user == null) return NotFound();
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

        // PATCH: api/users/change-password
        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (model == null ||
                string.IsNullOrWhiteSpace(model.OldPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest("Old and new password are required.");

            if (model.NewPassword.Length < 6)
                return BadRequest("New password must be at least 6 characters.");

            var success = await _userProfileService
                .ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (!success)
                return BadRequest("Old password is incorrect or new password is invalid.");

            return Ok("Password changed successfully.");
        }
    }
}

