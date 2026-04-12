// Base.Shared/DTOs/SystemAdminDTOs/AdminDashboardDTOs.cs

namespace Base.Shared.DTOs.SystemAdminDTOs
{
    // ══════════════════════════════════════════════════════════
    // DASHBOARD DTOs
    // ══════════════════════════════════════════════════════════

    public class AdminDashboardStatsDTO
    {
        public int TotalHospitals   { get; set; }
        public int ActiveHospitals  { get; set; }
        public int PendingReview    { get; set; }
        public int SuspendedHospitals { get; set; }
        public int TotalDonors      { get; set; }
        public int TotalDonations   { get; set; }
        public int TotalRequests    { get; set; }
    }

    public class RecentRegistrationDTO
    {
        public string   Id            { get; set; }
        public string   Name          { get; set; }
        public string   Email         { get; set; }
        public string?  Phone         { get; set; }
        public string   LicenseNumber { get; set; }
        public string   Status        { get; set; }
        public string?  CityName      { get; set; }
        public string?  GovernorateName { get; set; }
        public DateTime RegisteredAt  { get; set; }
    }

    public class RecentRegistrationsResult
    {
        public bool                        Success { get; set; }
        public string                      Message { get; set; }
        public List<RecentRegistrationDTO> Value   { get; set; } = new();
    }

    public class AdminActivityChartDTO
    {
        public List<string> Labels        { get; set; } = new();
        public List<int>    Registrations { get; set; } = new();
        public List<int>    Donations     { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════
    // DONORS DTOs
    // ══════════════════════════════════════════════════════════

    public class AdminDonorStatsDTO
    {
        public int TotalDonors      { get; set; }
        public int EligibleDonors   { get; set; }
        public int IneligibleDonors { get; set; }
        public int DonatedThisMonth { get; set; }
    }

    public class AdminDonorListResult
    {
        public bool                    Success    { get; set; }
        public string                  Message    { get; set; }
        public int                     Total      { get; set; }
        public int                     Page       { get; set; }
        public int                     Limit      { get; set; }
        public int                     TotalPages { get; set; }
        public List<AdminDonorSummaryDTO> Value   { get; set; } = new();
    }

    public class AdminDonorSummaryDTO
    {
        public string    DonorId          { get; set; }
        public string    FullName         { get; set; }
        public string?   Email            { get; set; }
        public string?   Phone            { get; set; }
        public string    BloodTypeName    { get; set; }
        public string?   CityName         { get; set; }
        public string?   GovernorateName  { get; set; }
        public DateOnly? LastDonationDate { get; set; }
        public int       TotalDonations   { get; set; }
        public bool      IsEligible       { get; set; }
        public bool      IsAvailable      { get; set; }
        public DateTime  RegisteredAt     { get; set; }
    }

    public class AdminDonorDetailResult
    {
        public bool                Success { get; set; }
        public string              Message { get; set; }
        public AdminDonorDetailDTO? Value  { get; set; }
    }

    public class AdminDonorDetailDTO : AdminDonorSummaryDTO
    {
        public string?   NationalId   { get; set; }
        public string?   Gender       { get; set; }
        public DateOnly? BirthDate    { get; set; }
        public decimal?  Latitude     { get; set; }
        public decimal?  Longitude    { get; set; }
        public string    BloodTypeId  { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    // USERS DTOs
    // ══════════════════════════════════════════════════════════

    public class AdminUserListResult
    {
        public bool                   Success    { get; set; }
        public string                 Message    { get; set; }
        public int                    Total      { get; set; }
        public int                    Page       { get; set; }
        public int                    Limit      { get; set; }
        public int                    TotalPages { get; set; }
        public List<AdminUserSummaryDTO> Value   { get; set; } = new();
    }

    public class AdminUserSummaryDTO
    {
        public string   Id           { get; set; }
        public string   FullName     { get; set; }
        public string   Email        { get; set; }
        public string?  PhoneNumber  { get; set; }
        public string   UserType     { get; set; }
        public bool     IsActive     { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class AdminUserDetailResult
    {
        public bool                 Success { get; set; }
        public string               Message { get; set; }
        public AdminUserDetailDTO?  Value   { get; set; }
    }

    public class AdminUserDetailDTO : AdminUserSummaryDTO
    {
        public bool    EmailConfirmed { get; set; }
        public string? HospitalName  { get; set; } // if HospitalAdmin
        public string? BloodTypeName { get; set; } // if Donor
    }

    public class UpdateUserStatusDTO
    {
        public bool IsActive { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    // ANALYTICS DTOs
    // ══════════════════════════════════════════════════════════

    public class AdminAnalyticsSummaryDTO
    {
        public int    TotalDonationsAllTime  { get; set; }
        public int    TotalRequestsAllTime   { get; set; }
        public int    TotalHospitals         { get; set; }
        public int    TotalDonors            { get; set; }
        public string MostRequestedBloodType { get; set; }
        public string MostActiveHospital     { get; set; }
    }

    public class AdminDonationsTrendDTO
    {
        public List<string> Labels    { get; set; } = new();
        public List<int>    Donations { get; set; } = new();
        public List<int>    Requests  { get; set; } = new();
    }

    public class AdminHospitalsByGovernorateResult
    {
        public bool                              Success { get; set; }
        public string                            Message { get; set; }
        public List<HospitalsByGovernorateDTO>   Value   { get; set; } = new();
    }

    public class HospitalsByGovernorateDTO
    {
        public string GovernorateId   { get; set; }
        public string GovernorateName { get; set; }
        public int    HospitalCount   { get; set; }
        public int    ActiveCount     { get; set; }
    }

    public class AdminBloodTypeDemandResult
    {
        public bool                        Success { get; set; }
        public string                      Message { get; set; }
        public List<BloodTypeDemandDTO>    Value   { get; set; } = new();
    }

    public class BloodTypeDemandDTO
    {
        public string BloodTypeId   { get; set; }
        public string BloodTypeName { get; set; }
        public int    RequestCount  { get; set; }
        public int    FulfilledCount { get; set; }
        public double FulfillmentRate { get; set; }
    }
}
