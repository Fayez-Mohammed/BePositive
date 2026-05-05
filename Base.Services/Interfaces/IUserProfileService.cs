using Base.Shared.DTOs;
using Base.Shared.Enums;

namespace Base.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserListDto> GetAllAsync(string? search, UserTypes? userType, bool? isActive, int page = 1, int pageSize = 20);
        Task<UserDto?> GetByIdAsync(string id);
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto?> UpdateAsync(string id, UpdateUserRequest request);
        Task<bool> ToggleActiveAsync(string id);
        Task<string> DeleteAsync(string id);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    }
}