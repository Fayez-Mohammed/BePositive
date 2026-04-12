// Base.Shared/DTOs/HospitalDTOs/HospitalProfileDTOs.cs

using System.ComponentModel.DataAnnotations;

namespace Base.Shared.DTOs.HospitalDTOs
{
    // ── GET response ──────────────────────────────────────────
    public class HospitalProfileDTO
    {
        public string   Id            { get; set; }
        public string   Name          { get; set; }
        public string   LicenseNumber { get; set; }  // read-only — not editable
        public string?  Email         { get; set; }
        public string?  Phone         { get; set; }
        public string?  Address       { get; set; }
        public decimal? Latitude      { get; set; }
        public decimal? Longitude     { get; set; }
        public string   Status        { get; set; }  // e.g. "Active"
        public DateTime JoinedDate    { get; set; }
        public CityInfoDTO?        City        { get; set; }
        public GovernorateInfoDTO? Governorate { get; set; }
    }

    public class CityInfoDTO
    {
        public string Id     { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }

    public class GovernorateInfoDTO
    {
        public string Id     { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }

    // ── GET result ────────────────────────────────────────────
    public class HospitalProfileResult
    {
        public bool               Success { get; set; }
        public string             Message { get; set; }
        public HospitalProfileDTO? Value  { get; set; }
    }

    // ── PATCH request ─────────────────────────────────────────
    public class UpdateHospitalProfileDTO
    {
        [Required]
        [MinLength(2)]
        [MaxLength(200)]
        public string  Name    { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone   { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? Email   { get; set; }

        public string?  CityId    { get; set; }
        public decimal? Latitude  { get; set; }
        public decimal? Longitude { get; set; }
    }

    // ── PATCH result ──────────────────────────────────────────
    public class UpdateHospitalProfileResult
    {
        public bool               Success { get; set; }
        public string             Message { get; set; }
        public HospitalProfileDTO? Value  { get; set; }
    }
}
