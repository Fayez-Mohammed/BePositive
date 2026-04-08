// Base.Services/Interfaces/HospitalInterfaces/IAnalyticsService.cs

using Base.Shared.DTOs.AnalyticsDTOs;

namespace Base.Services.Interfaces.HospitalInterfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsSummaryDTO>         GetSummaryAsync(string hospitalAdminUserId, AnalyticsPeriod period);
        Task<TrendsDTO>                   GetTrendsAsync(string hospitalAdminUserId, AnalyticsPeriod period);
        Task<BloodTypeDistributionResult> GetBloodTypeDistributionAsync(string hospitalAdminUserId, AnalyticsPeriod period);
    }
}
