// Base.Shared/DTOs/SystemAdminDTOs/AdminBloodRequestDTOs.cs

namespace Base.Shared.DTOs.SystemAdminDTOs
{
    // ══════════════════════════════════════════════════════════
    // ADMIN BLOOD REQUESTS DTOs
    // ══════════════════════════════════════════════════════════

    public class AdminBloodRequestListResult
    {
        public bool                           Success    { get; set; }
        public string                         Message    { get; set; }
        public int                            Total      { get; set; }
        public int                            Page       { get; set; }
        public int                            Limit      { get; set; }
        public int                            TotalPages { get; set; }
        public List<AdminBloodRequestSummaryDTO> Value   { get; set; } = new();
    }

    public class AdminBloodRequestSummaryDTO
    {
        public string   RequestId         { get; set; }
        public string   HospitalId        { get; set; }
        public string   HospitalName      { get; set; }
        public string?  HospitalCity      { get; set; }
        public string?  HospitalGovernorate { get; set; }
        public string   BloodTypeId       { get; set; }
        public string   BloodTypeName     { get; set; }
        public int      QuantityRequired  { get; set; }
        public int      QuantityFulfilled { get; set; }
        public double   ProgressPercent   { get; set; }
        public string   UrgencyLevel      { get; set; }
        public string   Status            { get; set; }
        public DateTime Deadline          { get; set; }
        public DateTime CreatedAt         { get; set; }
        public int      TotalResponses    { get; set; }
        public int      DonatedCount      { get; set; }
    }

    public class AdminBloodRequestDetailResult
    {
        public bool                       Success { get; set; }
        public string                     Message { get; set; }
        public AdminBloodRequestDetailDTO? Value   { get; set; }
    }

    public class AdminBloodRequestDetailDTO : AdminBloodRequestSummaryDTO
    {
        public string?  Note              { get; set; }
        public decimal? Latitude          { get; set; }
        public decimal? Longitude         { get; set; }
        public string?  HospitalPhone     { get; set; }
        public string?  HospitalEmail     { get; set; }
        public string?  HospitalAddress   { get; set; }
        public int      AcceptedCount     { get; set; }
        public int      ArrivedCount      { get; set; }
        public int      NoShowCount       { get; set; }
        public int      RejectedCount     { get; set; }
        public List<AdminDonorResponseDTO> DonorResponses { get; set; } = new();
    }

    public class AdminDonorResponseDTO
    {
        public string   ResponseId    { get; set; }
        public string   DonorId       { get; set; }
        public string   FullName      { get; set; }
        public string   BloodTypeName { get; set; }
        public string?  Phone         { get; set; }
        public string   Status        { get; set; }
        public DateTime RespondedAt   { get; set; }
        public double?  DistanceKm    { get; set; }
    }

    public class AdminBloodRequestStatsDTO
    {
        public int TotalRequests      { get; set; }
        public int OpenRequests       { get; set; }
        public int FulfilledRequests  { get; set; }
        public int CriticalRequests   { get; set; }
        public int TotalResponses     { get; set; }
        public int TotalDonations     { get; set; }
    }
    public class AllHospitalsDTO
    {
        public string? Name { get; set; }
        public string? Id { get; set; }

    }
    public class AllHospitalsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<AllHospitalsDTO>? Value { get; set; }
    }
}
