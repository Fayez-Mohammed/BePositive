// Base.Shared/DTOs/HospitalDTOs/DonorMonitoringDTOs.cs

namespace Base.Shared.DTOs.HospitalDTOs
{
    // ── Stats ─────────────────────────────────────────────────────
    public class DonorStatsDTO
    {
        public int EligibleDonors { get; set; }
        public int RecentlyDonated { get; set; }
        public int CurrentlyIneligible { get; set; }
    }

    // ── List query ────────────────────────────────────────────────
    public class GetDonorsQuery
    {
        public string? Search { get; set; }
        public string? BloodTypeId { get; set; }
        public string? Status { get; set; } // "Eligible" | "Ineligible"
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    // ── List result ───────────────────────────────────────────────
    public class DonorListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
        public List<DonorSummaryDTO> Value { get; set; } = new();
    }

    public class DonorSummaryDTO
    {
        public string DonorId { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string BloodTypeName { get; set; }
        public DateOnly? LastDonationDate { get; set; }
        public int TotalDonations { get; set; }
        public bool IsEligible { get; set; }
    }

    // ── Detail result ─────────────────────────────────────────────
    public class DonorDetailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DonorDetailDTO? Value { get; set; }
    }

    public class DonorDetailDTO
    {
        public string DonorId { get; set; }
        public string FullName { get; set; }
        public string BloodTypeName { get; set; }
        public string? CityName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateOnly? LastDonationDate { get; set; }
        public int TotalDonations { get; set; }
        public bool IsEligible { get; set; }
    }
}