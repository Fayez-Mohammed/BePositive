// Base.Shared/DTOs/AdminDTOs/AdminRequestDTOs.cs

using Base.Shared.Enums;

namespace Base.Shared.DTOs.AdminDTOs
{
    // ── PATCH status ──────────────────────────────────────────
    public class UpdateRequestStatusDTO
    {
        /// <summary>
        /// Allowed values: Cancelled, Expired
        /// Admin cannot set Fulfilled — that happens automatically.
        /// </summary>
        public RequestStatus Status { get; set; }

        public string? Note { get; set; }
    }

    // ── Single request detail ─────────────────────────────────
    public class AdminRequestDetailDTO
    {
        public string   Id                { get; set; } = null!;
        public string   HospitalId        { get; set; } = null!;
        public string   HospitalName      { get; set; } = null!;
        public string   BloodTypeId       { get; set; } = null!;
        public string   BloodTypeName     { get; set; } = null!;
        public int      QuantityRequired  { get; set; }
        public int      QuantityFulfilled { get; set; }
        public double   ProgressPercent   { get; set; }
        public string   UrgencyLevel      { get; set; } = null!;
        public string   Status            { get; set; } = null!;
        public string?  Note              { get; set; }
        public DateTime? Deadline         { get; set; }
        public int      TotalResponses    { get; set; }
        public int      Accepted          { get; set; }
        public int      Arrived           { get; set; }
        public int      Donated           { get; set; }
        public int      NoShow            { get; set; }
        public DateTime CreatedAt         { get; set; }
    }

    public class AdminRequestDetailResult
    {
        public bool                  Success { get; set; }
        public string                Message { get; set; } = null!;
        public AdminRequestDetailDTO? Value  { get; set; }
    }

    // ── List ──────────────────────────────────────────────────
    public class AdminRequestSummaryDTO
    {
        public string   Id                { get; set; } = null!;
        public string   HospitalId        { get; set; } = null!;
        public string   HospitalName      { get; set; } = null!;
        public string   BloodTypeId       { get; set; } = null!;
        public string   BloodTypeName     { get; set; } = null!;
        public int      QuantityRequired  { get; set; }
        public int      QuantityFulfilled { get; set; }
        public double   ProgressPercent   { get; set; }
        public string   UrgencyLevel      { get; set; } = null!;
        public string   Status            { get; set; } = null!;
        public string?  Note              { get; set; }
        public DateTime? Deadline         { get; set; }
        public int      TotalResponses    { get; set; }
        public DateTime CreatedAt         { get; set; }
    }

    public class AdminRequestListResult
    {
        public bool                         Success    { get; set; }
        public string                       Message    { get; set; } = null!;
        public List<AdminRequestSummaryDTO> Items      { get; set; } = new();
        public int                          Total      { get; set; }
        public int                          Page       { get; set; }
        public int                          Limit      { get; set; }
        public int                          TotalPages { get; set; }
    }

    // ── Stats ─────────────────────────────────────────────────
    public class AdminRequestStatsDTO
    {
        public int TotalRequests     { get; set; }
        public int OpenRequests      { get; set; }
        public int FulfilledRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int ExpiredRequests   { get; set; }
        public int CriticalRequests  { get; set; }
        public int TotalResponses    { get; set; }
    }

    public class AdminRequestStatsResult
    {
        public bool                  Success { get; set; }
        public string                Message { get; set; } = null!;
        public AdminRequestStatsDTO? Value   { get; set; }
    }

    // ── Hospitals dropdown ────────────────────────────────────
    public class AdminHospitalDropdownDTO
    {
        public string Id   { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class AdminHospitalDropdownResult
    {
        public bool                          Success { get; set; }
        public string                        Message { get; set; } = null!;
        public List<AdminHospitalDropdownDTO> Value  { get; set; } = new();
    }

    // ── Delete result ─────────────────────────────────────────
    public class AdminDeleteResult
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = null!;
    }
}
