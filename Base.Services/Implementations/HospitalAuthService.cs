// Base.Services/Implementations/HospitalAuthService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.HospitalModels;
using Base.Services.Interfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Base.Services.Implementations
{
    public class HospitalAuthService : IHospitalAuthService
    {
        
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        public HospitalAuthService(
       IOtpService otpService,
       ILogger<AuthService> logger, UserManager<ApplicationUser> userManager,
            AppDbContext context
       )
        {
          
            _otpService = otpService;
            _logger = logger;
            _context = context;
            _userManager = userManager;
         
        }



      
        public async Task<ApiResponseDTO> RegisterAsync(HospitalRegisterRequest request)
        {
            // 1. Check email not already taken
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return new ApiResponseDTO(409, "This email is already registered.");

            // 2. Check license number not already taken
            var licenseExists = await _context.Hospitals
                .AnyAsync(h => h.LicenseNumber == request.LicenseNumber && !h.IsDeleted);
            if (licenseExists)
                return new ApiResponseDTO(409, "A hospital with this license number already exists.");

            // 3. Validate CityId exists
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
            if (!cityExists)
                return new ApiResponseDTO(400, "Invalid city selected.");

            // 4. Create ApplicationUser for the hospital admin
            var adminUser = new ApplicationUser
            {
                FullName = request.HospitalName + " Admin",
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                Type = UserTypes.HospitalAdmin,
                IsActive = true,
                EmailConfirmed = false   // will be confirmed after OTP
            };

            var createUserResult = await _userManager.CreateAsync(adminUser, request.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                return new ApiResponseDTO(400, errors);
            }

            // 5. Assign HospitalAdmin role
            await _userManager.AddToRoleAsync(adminUser, UserTypes.HospitalAdmin.ToString());

            // 6. Create Hospital record
            var hospital = new Hospital
            {
                Name = request.HospitalName,
                LicenseNumber = request.LicenseNumber,
                Email = request.Email,
                Phone = request.PhoneNumber,
                CityId = request.CityId,
                Status = HospitalStatus.UnderReview,
                IsDeleted = false
            };
            _context.Hospitals.Add(hospital);

            // 7. Create HospitalAdmin linking user to hospital
            var hospitalAdmin = new HospitalAdmin
            {
                UserId = adminUser.Id,
                HospitalId = hospital.Id,
                IsDeleted = false
            };
            _context.HospitalAdmins.Add(hospitalAdmin);

            await _context.SaveChangesAsync();

            // 8. Send OTP for email verification
            var otpResult = await _otpService.GenerateAndSendOtpAsync(
                adminUser.Id,
                adminUser.Email,
                "verifyemail"
            );

            if (!otpResult.Success)
                _logger?.LogWarning(
                    "OTP failed after successful hospital registration - UserId: {UserId}",
                    adminUser.Id
                );

            // 9. Return response
            var response = new HospitalRegisterResponse
            {
                HospitalId = hospital.Id,
                AdminUserId = adminUser.Id,
                HospitalName = hospital.Name,
                Email = hospital.Email,
                Status = hospital.Status,
                Message = "Registration submitted. Please verify your email to continue."
            };

            return new ApiResponseDTO(201, "Hospital registered successfully.", response);
        }
        //public async Task<ApiResponseDTO> RegisterAsync(HospitalRegisterRequest request)
        //{
        //    // 1. Check email not already taken
        //    var existingUser = await _userManager.FindByEmailAsync(request.Email);
        //    if (existingUser is not null)
        //        return new ApiResponseDTO(409, "This email is already registered.");

        //    // 2. Check license number not already taken
        //    var licenseExists = await _context.Hospitals
        //        .AnyAsync(h => h.LicenseNumber == request.LicenseNumber && !h.IsDeleted);
        //    if (licenseExists)
        //        return new ApiResponseDTO(409, "A hospital with this license number already exists.");

        //    // 3. Validate CityId exists
        //    var cityExists = await _context.Cities
        //        .AnyAsync(c => c.Id == request.CityId);
        //    if (!cityExists)
        //        return new ApiResponseDTO(400, "Invalid city selected.");

        //    // 4. Create ApplicationUser for the hospital admin
        //    var adminUser = new ApplicationUser
        //    {
        //        FullName = request.HospitalName + " Admin",
        //        Email = request.Email,
        //        UserName = request.Email,
        //        PhoneNumber = request.PhoneNumber,
        //        Type = UserTypes.HospitalAdmin,
        //        IsActive = true
        //    };

        //    var createUserResult = await _userManager.CreateAsync(adminUser, request.Password);
        //    if (!createUserResult.Succeeded)
        //    {
        //        var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
        //        return new ApiResponseDTO(400, errors);
        //    }
        //    await _userManager.AddToRoleAsync(adminUser, UserTypes.HospitalAdmin.ToString());

        //    // 5. Create Hospital record
        //    var hospital = new Hospital
        //    {
        //        Name = request.HospitalName,
        //        LicenseNumber = request.LicenseNumber,
        //        Email = request.Email,
        //        Phone = request.PhoneNumber,
        //        CityId = request.CityId,
        //        Status = HospitalStatus.UnderReview.ToString(),
        //        IsDeleted = false
        //    };
        //    _context.Hospitals.Add(hospital);

        //    // 6. Create HospitalAdmin linking user to hospital
        //    var hospitalAdmin = new HospitalAdmin
        //    {
        //        UserId = adminUser.Id,
        //        HospitalId = hospital.Id,
        //        IsDeleted = false
        //    };
        //    _context.HospitalAdmins.Add(hospitalAdmin);

        //    await _context.SaveChangesAsync();

        //    // 7. Build and return response
        //    var response = new HospitalRegisterResponse
        //    {
        //        HospitalId = hospital.Id,
        //        AdminUserId = adminUser.Id,
        //        HospitalName = hospital.Name,
        //        Email = hospital.Email,
        //        Status = hospital.Status,
        //        Message = "Registration submitted. Your account is under review."
        //    };

        //    return new ApiResponseDTO(201, "Hospital registered successfully.", response);
        //}
    }
}