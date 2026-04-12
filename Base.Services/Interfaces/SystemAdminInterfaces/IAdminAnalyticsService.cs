// Base.Services/Interfaces/IAdminAnalyticsService.cs

using Base.Shared.DTOs.SystemAdminDTOs;

namespace Base.Services.Interfaces
{
    public interface IAdminAnalyticsService
    {
        Task<AdminAnalyticsSummaryDTO>         GetSummaryAsync();
        Task<AdminDonationsTrendDTO>           GetDonationsTrendAsync(string period = "LastYear");
        Task<AdminHospitalsByGovernorateResult> GetHospitalsByGovernorateAsync();
        Task<AdminBloodTypeDemandResult>        GetBloodTypeDemandAsync();
    }
}
