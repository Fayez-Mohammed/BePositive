using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Base.Services.Helpers;
using Base.Services.Interfaces;
using Base.Shared.DTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepositoryProject.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.Users
                //.Include(u => u.Farms)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user?.ToUserDto();
        }
        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            var user = request.ToApplicationUser();
            user.IsActive = true;
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return user.ToUserDto();
        }
        public async Task<UserDto?> UpdateAsync(string id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            //if (request.UserType.HasValue)
            //    user.Type = request.UserType ?? user.Type;

            //if (request.IsActive.HasValue)
            //    user.IsActive = request.IsActive.Value;
            if (request.Email != null)
            {
                user.Email = request.Email;
                user.UserName = request.Email; // Assuming username is the same as email

            }
            if(request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.ImagePath))
                user.ImagePath = request.ImagePath;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return user.ToUserDto();
        }
        public async Task<bool> ToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        public async Task<string> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return "User Not Found";
            if(await _userManager.IsInRoleAsync(user,UserTypes.SystemAdmin.ToString()) )
                return "You cannot delete a system admin.";
            if(user.IsDeleted) return "User is already marked as deleted.";
            user.IsDeleted = true;
            user.FullName=user.FullName+" (Deleted)";
            user.Email=user.Email+" (Deleted)";

            await _unitOfWork.CompleteAsync();
            return "User marked as deleted.";


           // var result = await _userManager.DeleteAsync(user);
            //try
            //{
            //    await _unitOfWork.CompleteAsync();
            //}
            //catch (DbUpdateException ex)
            //{
            //    // ضع نقطة توقف (Breakpoint) هنا لرؤية تفاصيل الخطأ الفعلي من SQL Server
            //    Console.WriteLine(ex.InnerException?.Message);
            //    throw new InvalidOperationException("Cannot delete this record because it has related data.");
            //}
            //if (result.Succeeded)
            //                    return "User deleted successfully.";
            //else
            //    return "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }
        //public async Task<bool> ChangePasswordAsync(string userId, string newPassword)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user is null) return false;

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        //    return result.Succeeded;
        //}
        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            return result.Succeeded;
        }
        public async Task<UserListDto> GetAllAsync(string? search, UserTypes? userType, bool? isActive, int page, int pageSize)
        {
            var query = _userManager.Users.Where(u=>u.IsDeleted==false).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            if (userType.HasValue)
                query = query.Where(u => u.Type == userType.Value&&!u.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var total = await query.CountAsync();

            //query.Include(u => u.Farms);

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).OrderByDescending(u => u.DateOfCreattion)
                .ToListAsync();

            var userDtos = users.ToUserDtoSet();// _mapper.Map<List<UserDto>>(users);

            return new UserListDto
            {
                Users = userDtos.ToList(),
                TotalCount = total,
                FilteredCount = userDtos.Count
            };
        }

    }
}

