// Base.Services/Implementations/HospitalImplementations/HospitalProfileService.cs

using Base.DAL.Contexts;
using Base.DAL.Models.HospitalModels;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations.HospitalImplementations
{
    public class HospitalProfileService : IHospitalProfileService
    {
        private readonly AppDbContext _context;

        public HospitalProfileService(AppDbContext context)
        {
            _context = context;
        }

        // ── Helper: get hospitalId from admin user ────────────
        private async Task<string> GetHospitalIdAsync(string hospitalAdminUserId)
        {
            var admin = await _context.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha =>
                    ha.UserId == hospitalAdminUserId && !ha.IsDeleted);

            if (admin == null)
                throw new UnauthorizedAccessException(
                    "No hospital admin record found.");

            return admin.HospitalId;
        }

        // ── Helper: map Hospital to DTO ───────────────────────
        private static HospitalProfileDTO MapToDTO(Hospital h)
        {
            return new HospitalProfileDTO
            {
                Id            = h.Id,
                Name          = h.Name,
                LicenseNumber = h.LicenseNumber,
                Email         = h.Email,
                Phone         = h.Phone,
                Address       = h.Address,
                Latitude      = h.Latitude,
                Longitude     = h.Longitude,
                Status        = h.Status.ToString(),
                JoinedDate    = h.DateOfCreattion,
                City          = h.City == null ? null : new CityInfoDTO
                {
                    Id     = h.City.Id,
                    NameEn = h.City.NameEn,
                    NameAr = h.City.NameAr
                },
                Governorate = h.City?.Governorate == null ? null
                    : new GovernorateInfoDTO
                    {
                        Id     = h.City.Governorate.Id,
                        NameEn = h.City.Governorate.NameEn,
                        NameAr = h.City.Governorate.NameAr
                    }
            };
        }

        // ── GET /api/hospital/profile ─────────────────────────
        public async Task<HospitalProfileResult> GetProfileAsync(
            string hospitalAdminUserId)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var hospital = await _context.Hospitals
                .AsNoTracking()
                .Include(h => h.City)
                .ThenInclude(c => c.Governorate)
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId && !h.IsDeleted);

            if (hospital == null)
                return new HospitalProfileResult
                {
                    Success = false,
                    Message = "Hospital not found."
                };

            return new HospitalProfileResult
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Value   = MapToDTO(hospital)
            };
        }

        // ── PATCH /api/hospital/profile ───────────────────────
        public async Task<UpdateHospitalProfileResult> UpdateProfileAsync(
            string hospitalAdminUserId,
            UpdateHospitalProfileDTO dto)
        {
            var hospitalId = await GetHospitalIdAsync(hospitalAdminUserId);

            var hospital = await _context.Hospitals
                .Include(h => h.City)
                .ThenInclude(c => c.Governorate)
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId && !h.IsDeleted);

            if (hospital == null)
                return new UpdateHospitalProfileResult
                {
                    Success = false,
                    Message = "Hospital not found."
                };

            // Validate city if provided
            if (!string.IsNullOrWhiteSpace(dto.CityId))
            {
                var cityExists = await _context.Cities
                    .AnyAsync(c => c.Id == dto.CityId);

                if (!cityExists)
                    throw new ArgumentException(
                        "Invalid city selected.");
            }

            // Apply updates — LicenseNumber is never updated
            hospital.Name      = dto.Name;
            hospital.Address   = dto.Address;
            hospital.Phone     = dto.Phone;
            hospital.Email     = dto.Email;
            hospital.Latitude  = dto.Latitude;
            hospital.Longitude = dto.Longitude;

            if (!string.IsNullOrWhiteSpace(dto.CityId))
                hospital.CityId = dto.CityId;

            hospital.UpdatedById  = hospitalAdminUserId;
            hospital.DateOfUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload with navigation properties for response
            var updated = await _context.Hospitals
                .AsNoTracking()
                .Include(h => h.City)
                .ThenInclude(c => c.Governorate)
                .FirstOrDefaultAsync(h => h.Id == hospitalId);

            return new UpdateHospitalProfileResult
            {
                Success = true,
                Message = "Profile updated successfully.",
                Value   = MapToDTO(updated!)
            };
        }
    }
}
