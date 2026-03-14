// Base.Services/Implementations/HospitalManagementService.cs

using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.SystemAdminDTOs;
using Base.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Implementations
{
    public class HospitalManagementService : IHospitalManagementService
    {
        private readonly AppDbContext _context;

        public HospitalManagementService(AppDbContext context)
        {
            _context = context;
        }

        // ── Get All ───────────────────────────────────────────────
        public async Task<HospitalListResult> GetAllHospitalsAsync(
            HospitalStatus? status, int page, int limit)
        {
            var query = _context.Hospitals
                .Include(h => h.City).ThenInclude(c => c.Governorate)
                .Include(h => h.Admin).ThenInclude(a => a.User)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(h => h.Status == status.Value);

            var total = await query.CountAsync();

            var hospitals = await query
                .OrderByDescending(h => h.DateOfCreattion)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(h => new HospitalSummaryDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    LicenseNumber = h.LicenseNumber,
                    Email = h.Email,
                    Phone = h.Phone,
                    Status = h.Status,
                    DateOfCreation = h.DateOfCreattion,
                    City = h.City == null ? null : new CityDto
                    {
                        Id = h.City.Id,
                        NameEn = h.City.NameEn,
                        NameAr = h.City.NameAr
                    },
                    Governorate = h.City == null || h.City.Governorate == null ? null : new GovernorateDto
                    {
                        Id = h.City.Governorate.Id,
                        NameEn = h.City.Governorate.NameEn,
                        NameAr = h.City.Governorate.NameAr
                    },
                    Admin = h.Admin == null ? null : new HospitalAdminDto
                    {
                        Id = h.Admin.Id,
                        JobTitle = h.Admin.JobTitle,
                        JobDescription = h.Admin.JobDescription,
                        User = h.Admin.User == null ? null : new AdminUserDto
                        {
                            Id = h.Admin.User.Id,
                            FullName = h.Admin.User.FullName,
                            Email = h.Admin.User.Email,
                            PhoneNumber = h.Admin.User.PhoneNumber
                        }
                    }
                })
                .ToListAsync();

            return new HospitalListResult
            {
                Success = true,
                Message = "Hospitals retrieved successfully.",
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                Data = hospitals
            };
        }

        // ── Get By Id ─────────────────────────────────────────────
        public async Task<HospitalDetailResult> GetHospitalByIdAsync(string id)
        {
            var h = await _context.Hospitals
                .Include(h => h.City).ThenInclude(c => c.Governorate)
                .Include(h => h.Admin).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (h == null)
                return new HospitalDetailResult
                {
                    Success = false,
                    Message = "Hospital not found."
                };

            return new HospitalDetailResult
            {
                Success = true,
                Message = "Hospital retrieved successfully.",
                Data = new HospitalDetailDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    LicenseNumber = h.LicenseNumber,
                    Email = h.Email,
                    Phone = h.Phone,
                    Address = h.Address,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    Status = h.Status,
                    IsDeleted = h.IsDeleted,
                    DateOfCreation = h.DateOfCreattion,
                    DateOfUpdate = h.DateOfUpdate,
                    City = h.City == null ? null : new CityDto
                    {
                        Id = h.City.Id,
                        NameEn = h.City.NameEn,
                        NameAr = h.City.NameAr
                    },
                    Governorate = h.City?.Governorate == null ? null : new GovernorateDto
                    {
                        Id = h.City.Governorate.Id,
                        NameEn = h.City.Governorate.NameEn,
                        NameAr = h.City.Governorate.NameAr
                    },
                    Admin = h.Admin == null ? null : new HospitalAdminDto
                    {
                        Id = h.Admin.Id,
                        JobTitle = h.Admin.JobTitle,
                        JobDescription = h.Admin.JobDescription,
                        User = h.Admin.User == null ? null : new AdminUserDto
                        {
                            Id = h.Admin.User.Id,
                            FullName = h.Admin.User.FullName,
                            Email = h.Admin.User.Email,
                            PhoneNumber = h.Admin.User.PhoneNumber
                        }
                    }
                }
            };
        }

        // ── Update Status ─────────────────────────────────────────
        public async Task<HospitalStatusResult> UpdateStatusAsync(
            string id, HospitalStatus newStatus)
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == id);
            if (hospital == null)
                return new HospitalStatusResult
                {
                    Success = false,
                    Message = "Hospital not found."
                };

            var previous = hospital.Status;
            hospital.Status = newStatus;
            await _context.SaveChangesAsync();

            return new HospitalStatusResult
            {
                Success = true,
                Message = $"Hospital status updated to {newStatus} successfully.",
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                PreviousStatus = previous,
                NewStatus = newStatus
            };
        }

        // ── Activate ──────────────────────────────────────────────
        public async Task<HospitalStatusResult> ActivateAsync(string id)
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == id);
            if (hospital == null)
                return new HospitalStatusResult { Success = false, Message = "Hospital not found." };

            if (hospital.Status == HospitalStatus.Active)
                return new HospitalStatusResult { Success = false, Message = "Hospital is already active." };

            var previous = hospital.Status;
            hospital.Status = HospitalStatus.Active;
            await _context.SaveChangesAsync();

            return new HospitalStatusResult
            {
                Success = true,
                Message = "Hospital activated successfully.",
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                PreviousStatus = previous,
                NewStatus = HospitalStatus.Active
            };
        }

        // ── Suspend ───────────────────────────────────────────────
        public async Task<HospitalStatusResult> SuspendAsync(string id)
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == id);
            if (hospital == null)
                return new HospitalStatusResult { Success = false, Message = "Hospital not found." };

            if (hospital.Status == HospitalStatus.Suspended)
                return new HospitalStatusResult { Success = false, Message = "Hospital is already suspended." };

            var previous = hospital.Status;
            hospital.Status = HospitalStatus.Suspended;
            await _context.SaveChangesAsync();

            return new HospitalStatusResult
            {
                Success = true,
                Message = "Hospital suspended successfully.",
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                PreviousStatus = previous,
                NewStatus = HospitalStatus.Suspended
            };
        }

        // ── Soft Delete ───────────────────────────────────────────
        public async Task<HospitalDeleteResult> DeleteAsync(string id, string deletedByUserId)
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == id);
            if (hospital == null)
                return new HospitalDeleteResult { Success = false, Message = "Hospital not found." };

            hospital.IsDeleted = true;
            hospital.DeletedAt = DateTime.UtcNow;
            hospital.DeletedById = deletedByUserId;
            await _context.SaveChangesAsync();

            return new HospitalDeleteResult
            {
                Success = true,
                Message = "Hospital deleted successfully.",
                HospitalId = hospital.Id,
                HospitalName = hospital.Name
            };
        }

        // ── Stats ─────────────────────────────────────────────────
        public async Task<HospitalStatsResult> GetStatsAsync()
        {
            var total = await _context.Hospitals.CountAsync();
            var underReview = await _context.Hospitals.CountAsync(h => h.Status == HospitalStatus.UnderReview);
            var active = await _context.Hospitals.CountAsync(h => h.Status == HospitalStatus.Active);
            var suspended = await _context.Hospitals.CountAsync(h => h.Status == HospitalStatus.Suspended);

            return new HospitalStatsResult
            {
                Success = true,
                Total = total,
                UnderReview = underReview,
                Active = active,
                Suspended = suspended
            };
        }
    }
}