// Base.Shared/DTOs/HospitalDTOs/HospitalManagementDTOs.cs

using Base.Shared.Enums;

namespace Base.Shared.DTOs.SystemAdminDTOs
{
    // ── Shared sub-objects ────────────────────────────────────────
    public class CityDto
    {
        public string Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }

    public class GovernorateDto
    {
        public string Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }

    public class AdminUserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class HospitalAdminDto
    {
        public string Id { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public AdminUserDto User { get; set; }
    }

    // ── GetAllHospitals ───────────────────────────────────────────
    public class HospitalSummaryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public HospitalStatus Status { get; set; }
        public DateTime DateOfCreation { get; set; }
        public CityDto? City { get; set; }
        public GovernorateDto? Governorate { get; set; }
        public HospitalAdminDto? Admin { get; set; }
    }

    public class HospitalListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
        public List<HospitalSummaryDto> Data { get; set; } = new();
    }

    // ── GetHospitalById ───────────────────────────────────────────
    public class HospitalDetailDto : HospitalSummaryDto
    {
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateOfUpdate { get; set; }
    }

    public class HospitalDetailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HospitalDetailDto? Data { get; set; }
    }

    // ── UpdateStatus / Activate / Suspend ─────────────────────────
    public class HospitalStatusResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? HospitalId { get; set; }
        public string? HospitalName { get; set; }
        public HospitalStatus? PreviousStatus { get; set; }
        public HospitalStatus? NewStatus { get; set; }
    }

    // ── Delete ────────────────────────────────────────────────────
    public class HospitalDeleteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? HospitalId { get; set; }
        public string? HospitalName { get; set; }
    }

    // ── Stats ─────────────────────────────────────────────────────
    public class HospitalStatsResult
    {
        public bool Success { get; set; }
        public int Total { get; set; }
        public int UnderReview { get; set; }
        public int Active { get; set; }
        public int Suspended { get; set; }
    }

    // ── Request ───────────────────────────────────────────────────
    public class UpdateHospitalStatusRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public HospitalStatus Status { get; set; }
    }
}