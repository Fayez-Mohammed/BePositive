// Base.Shared/DTOs/HospitalDTOs/BloodRequestResponseDTO.cs

using Base.Shared.Enums;

namespace Base.Shared.DTOs.HospitalDTOs
{
    public class BloodRequestResponseDTO
    {
        public string Id { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
        public string BloodTypeId { get; set; }
        public string BloodTypeName { get; set; }
        public int QuantityRequired { get; set; }
        public int QuantityFulfilled { get; set; }
        public UrgencyLevel UrgencyLevel { get; set; }
        public RequestStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Get All Requests — Query filters ──────────────────────────
    public class GetBloodRequestsQuery
    {
        public string? Search { get; set; }  // search by id or note
        public RequestStatus? Status { get; set; }  // Open, Fulfilled, Cancelled, Expired
        public UrgencyLevel? UrgencyLevel { get; set; }
        public string? BloodTypeId { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    // ── Get All Requests — Response ───────────────────────────────
    public class BloodRequestListResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
        public List<BloodRequestSummaryDTO> Value { get; set; } = new();
    }

    public class BloodRequestSummaryDTO
    {
        public string Id { get; set; }
        public string HospitalId { get; set; }
        public string BloodTypeId { get; set; }
        public string BloodTypeName { get; set; }
        public int QuantityRequired { get; set; }
        public int QuantityFulfilled { get; set; }
        public double ProgressPercent { get; set; } // e.g. 66.6
        public UrgencyLevel UrgencyLevel { get; set; }
        public RequestStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Get Single Request Detail ─────────────────────────────────
    public class BloodRequestDetailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BloodRequestDetailDTO? Value { get; set; }
    }

    public class BloodRequestDetailDTO : BloodRequestSummaryDTO
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int Responses { get; set; } // total donor responses count
        public int Accepted { get; set; } // accepted responses count
        public int Arrived { get; set; } // arrived responses count
        public int Donated { get; set; } // donated responses count
        public int NoShow { get; set; } // no show responses count
    }

    // ── Update Request ────────────────────────────────────────────
    public class UpdateBloodRequestDTO
    {
        public int? QuantityRequired { get; set; }
        public UrgencyLevel? UrgencyLevel { get; set; }
        public string? Note { get; set; }
        public DateTime? Deadline { get; set; }
        public RequestStatus? Status { get; set; } // Cancel only from here
    }
    public class BloodTypeDTO
    {
        public string Id { get; set; }
        public string TypeName { get; set; }
    }
}