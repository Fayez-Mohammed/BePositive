// Base.Services/Interfaces/IAdminUserService.cs

using Base.Shared.DTOs.SystemAdminDTOs;

namespace Base.Services.Interfaces
{
    public interface IAdminUserService
    {
        Task<AdminUserListResult>   GetAllUsersAsync(
            string? search,
            string? userType,
            int     page,
            int     limit);
        Task<AdminUserDetailResult> GetUserByIdAsync(string userId);
        Task<bool>                  UpdateUserStatusAsync(
            string userId,
            bool   isActive);
    }
}
