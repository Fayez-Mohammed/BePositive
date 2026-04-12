// Base.Services/Implementations/AdminUserService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.BaseModels;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext          _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context     = context;
            _userManager = userManager;
        }

        // ── GET /api/admin/users ──────────────────────────────
        public async Task<AdminUserListResult> GetAllUsersAsync(
            string? search,
            string? Type,
            int     page,
            int     limit)
        {
            var q = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                q = q.Where(u =>
                    u.FullName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    (u.PhoneNumber != null &&
                     u.PhoneNumber.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(Type))
                q = q.Where(u =>
                    u.Type.ToString() == Type);

            var total = await q.CountAsync();

            var users = await q
                .OrderByDescending(u => u.LockoutEnd)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new AdminUserSummaryDTO
                {
                    Id           = u.Id,
                    FullName     = u.FullName,
                    Email        = u.Email ?? "",
                    PhoneNumber  = u.PhoneNumber,
                    UserType     = u.Type.ToString(),
                    IsActive     = u.LockoutEnd == null ||
                                   u.LockoutEnd < DateTimeOffset.UtcNow,
                    RegisteredAt = u.LockoutEnd.HasValue
                        ? DateTime.UtcNow : DateTime.UtcNow
                })
                .ToListAsync();

            return new AdminUserListResult
            {
                Success    = true,
                Message    = "Users retrieved successfully.",
                Total      = total,
                Page       = page,
                Limit      = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                Value      = users
            };
        }

        // ── GET /api/admin/users/{id} ─────────────────────────
        public async Task<AdminUserDetailResult> GetUserByIdAsync(
            string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AdminUserDetailResult
                {
                    Success = false,
                    Message = "User not found."
                };

            bool isActive = user.LockoutEnd == null ||
                            user.LockoutEnd < DateTimeOffset.UtcNow;

            // Get hospital name if HospitalAdmin
            string? hospitalName = null;
            if (user.Type == Base.Shared.Enums.UserTypes.HospitalAdmin)
            {
                hospitalName = await _context.HospitalAdmins
                    .AsNoTracking()
                    .Where(ha => ha.UserId == userId && !ha.IsDeleted)
                    .Select(ha => ha.Hospital.Name)
                    .FirstOrDefaultAsync();
            }

            // Get blood type if Donor
            string? bloodTypeName = null;
            if (user.Type == Base.Shared.Enums.UserTypes.Donor)
            {
                bloodTypeName = await _context.Donors
                    .AsNoTracking()
                    .Where(d => d.UserId == userId && !d.IsDeleted)
                    .Select(d => d.BloodType.TypeName)
                    .FirstOrDefaultAsync();
            }

            return new AdminUserDetailResult
            {
                Success = true,
                Message = "User retrieved successfully.",
                Value   = new AdminUserDetailDTO
                {
                    Id             = user.Id,
                    FullName       = user.FullName,
                    Email          = user.Email ?? "",
                    PhoneNumber    = user.PhoneNumber,
                    UserType       = user.Type.ToString(),
                    IsActive       = isActive,
                    EmailConfirmed = user.EmailConfirmed,
                    HospitalName   = hospitalName,
                    BloodTypeName  = bloodTypeName,
                    RegisteredAt   = DateTime.UtcNow
                }
            };
        }

        // ── PATCH /api/admin/users/{id}/status ────────────────
        public async Task<bool> UpdateUserStatusAsync(
            string userId,
            bool   isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (isActive)
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else
            {
                // Lock the user for 100 years (effectively permanent)
                await _userManager.SetLockoutEndDateAsync(
                    user,
                    DateTimeOffset.UtcNow.AddYears(100));
            }

            return true;
        }
    }
}
